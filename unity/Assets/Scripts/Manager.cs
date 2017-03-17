using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SFB;

public class Manager : MonoBehaviour {
    List<ObjectSelector> queue;
    HashSet<ObjectSelector> queueSet;
    Dictionary<ObjectSelector, string> objToGroupName;
    Dictionary<ObjectSelector, Vector3> objToOrigCentroid;
    Dictionary<string, HashSet<ObjectSelector>> groupNameToObjs;
    Vector3 baseOffset;
    readonly string GROUP_SUFFIX = "_parts.csv";

	void Start() {
        cameraMovement = GameObject.Find("Main Camera").GetComponent<CameraMovement>();
        labelInputField = GameObject.Find("Canvas/LabelInputField").GetComponent<InputField>();
        // dialogPanel = GameObject.Find("Canvas/DialogPanel");
        // dialogPanel.SetActive(false);

        queue = new List<ObjectSelector>();
        queueSet = new HashSet<ObjectSelector>();
        objToGroupName = new Dictionary<ObjectSelector, string>();
        objToOrigCentroid = new Dictionary<ObjectSelector, Vector3>();
        groupNameToObjs = new Dictionary<string, HashSet<ObjectSelector>>();
	}

    void Load(string objPath, string groupPath) {
        GameObject parentObj = OBJLoader.LoadOBJFile(objPath);

        List<GameObject> children = new List<GameObject>();
        foreach (Transform childTrans in parentObj.transform) {
            children.Add(childTrans.gameObject);
        }


        Dictionary<string, ObjectSelector> nameToObj = new Dictionary<string, ObjectSelector>();
        ObjectSelector.manager = this;
        foreach (GameObject child in children) {
            Mesh mesh = child.GetComponent<MeshFilter>().mesh;
            Vector3[] vertices = mesh.vertices;
            Vector3 centroid = new Vector3();
            foreach (Vector3 vertex in vertices) {
                centroid += vertex;
            }
            centroid /= vertices.Length;
            int i = 0;
            foreach (Vector3 vertex in vertices) {
                vertices[i] += centroid;
                i++;
            }
            mesh.vertices = vertices;
            mesh.RecalculateBounds();
            Vector3 newCentroid = centroid + centroid;

            child.AddComponent<MeshCollider>();
            ObjectSelector childObjSelector = child.AddComponent<ObjectSelector>();
            objToOrigCentroid[childObjSelector] = centroid;
            childObjSelector.centroid = newCentroid;
            objToGroupName[childObjSelector] = child.name;
            nameToObj[child.name] = childObjSelector;
        }

        using (System.IO.StreamReader file = new System.IO.StreamReader(groupPath)) {
            int anonIndex = 1;
            while (!file.EndOfStream) {
                string[] splits = file.ReadLine().Split(',');
                HashSet<ObjectSelector> objs = new HashSet<ObjectSelector>();
                if (splits.Length > 1) {
                    string groupName = "anon_" + anonIndex;
                    anonIndex++;
                    foreach (string name in splits) {
                        ObjectSelector obj = nameToObj[name];
                        objToGroupName[obj] = groupName;
                        objs.Add(obj);
                    }
                    groupNameToObjs[groupName] = objs;
                } else { // only one object in this group, use its own name as group name
                    string name = splits[0];
                    objs.Add(nameToObj[name]);
                    groupNameToObjs[name] = objs;
                }
            }
        }

        cameraMovement.LookAtZoom(children);
    }

    public void Hover(ObjectSelector obj) {
        hoverObj = obj;
        obj.UpdateColor();
    }

    public void Unhover(ObjectSelector obj) {
        if (hoverObj == obj) {
            hoverObj = null;
            obj.UpdateColor();
        }
    }

    void AddToQueue(ObjectSelector obj) {
        queue.Add(obj);
        queueSet.Add(obj);
        obj.UpdateColor();
        if (queue.Count == 1) { // first one, set base
            baseOffset = obj.centroid - objToOrigCentroid[obj];
            return;
        }
        Vector3 offset = objToOrigCentroid[obj] - obj.centroid + baseOffset;
        Vector3 newCentroid = obj.centroid + offset;
        Mesh mesh = obj.gameObject.GetComponent<MeshFilter>().mesh;
        Vector3[] vertices = mesh.vertices;
        int i = 0;
        foreach (Vector3 vertex in vertices) {
            vertices[i] += offset;
            i++;
        }
        mesh.vertices = vertices;
        mesh.RecalculateBounds();
        obj.centroid = newCentroid;
    }

    CameraMovement cameraMovement = null;
    public ObjectSelector hoverObj = null;
    public ObjectSelector selectedObj = null;
    public bool hasSelectedGroup = false;
    string selectedGroupName = System.String.Empty;
    public HashSet<ObjectSelector> selectedGroup = null;
    Vector3 cameraPivot;
    InputField labelInputField = null;
    GameObject dialogPanel = null;

    bool IsTextFieldFocused() {
        return labelInputField.isFocused;
    }

    bool IsControlPressed() {
        return Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
    }

    public void HandleRename(string name) {
        if (name == selectedGroupName) { // name not changed
            labelInputField.DeactivateInputField();
            return;
        }
        if (groupNameToObjs.ContainsKey(name)) {
            labelInputField.ActivateInputField();
            labelInputField.text = selectedGroupName;
            // dialogPanel.SetActive(true);
            // EditorUtility.DisplayDialog("Warning", "Name conflicts with another group!", "Continue");
            return;
        }
        groupNameToObjs.Remove(selectedGroupName);
        selectedGroupName = name;
        foreach (ObjectSelector obj in selectedGroup) {
            objToGroupName[obj] = name;
        }
        groupNameToObjs[selectedGroupName] = selectedGroup;
        labelInputField.DeactivateInputField();
    }

    void RunCommand(string command) {
        System.Diagnostics.Process process = new System.Diagnostics.Process();
        System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
        startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
        startInfo.FileName = "cmd.exe";
        startInfo.Arguments = System.String.Format("/C {0}", command);
        process.StartInfo = startInfo;
        process.Start();
        process.WaitForExit();
    }

    public void HandleLoad() {
        try {
            var inExtensions = new [] {
                new ExtensionFilter("3D Model Files", "obj"),
            };
            string objPath = StandaloneFileBrowser.OpenFilePanel("Select Input File", "./", inExtensions, false)[0];
            string groupPath = objPath + GROUP_SUFFIX;
            RunCommand(System.String.Format("python Assets/Python/group_parts.py {0} {1}", objPath, groupPath));
            Load(objPath, groupPath);
        } catch (System.ArgumentException) {
        }
    }

    public void HandleExport() {
        var outExtensions = new [] {
            new ExtensionFilter("Sequence File", "out"),
        };
        string path = StandaloneFileBrowser.SaveFilePanel("Enter Output Path", "./", "sequence", outExtensions);
        using (System.IO.StreamWriter file = new System.IO.StreamWriter(path)) {
            foreach (ObjectSelector selector in queue) {
                file.WriteLine(selector.gameObject.name + "," + objToGroupName[selector]);
            }
        }
    }

    void SwapSelect(ObjectSelector newObj) {
        ObjectSelector oldSelectedObj = selectedObj;
        HashSet<ObjectSelector> oldGroup = selectedGroup;
        selectedObj = newObj;
        hasSelectedGroup = false;
        selectedGroupName = System.String.Empty;
        selectedGroup = null;
        if (selectedObj != null) {
            selectedGroupName = objToGroupName[selectedObj];
            selectedGroup = groupNameToObjs[selectedGroupName];
            ObjectSelector.UpdateGroupColor(selectedGroup);
            selectedObj.UpdateColor();
        }
        if (oldSelectedObj != null) {
            oldSelectedObj.UpdateColor();
        }
        if (oldGroup != null) {
            ObjectSelector.UpdateGroupColor(oldGroup);
        }
    }

    void LateUpdate() {
        if (selectedObj == null && !hasSelectedGroup) { // set hover text only if no obj or group selected
            labelInputField.text = (hoverObj == null) ? System.String.Empty : objToGroupName[hoverObj];
        }
        if (IsTextFieldFocused()) { // we're typing in a text field, ignore any keyboard inputs
            return;
        }
        if (selectedObj != null) { // single object selection mode
            if (Input.GetKey(KeyCode.Return) && !queueSet.Contains(selectedObj)) {
                AddToQueue(selectedObj);
            } else if (Input.GetKey(KeyCode.G)) { // select object's group
                hasSelectedGroup = true;
                selectedGroupName = objToGroupName[selectedObj];
                selectedGroup = groupNameToObjs[selectedGroupName];
                selectedObj = null;
                ObjectSelector.UpdateGroupColor(selectedGroup);
                cameraPivot = new Vector3();
                foreach (ObjectSelector obj in selectedGroup) {
                    cameraPivot += obj.gameObject.transform.TransformPoint(obj.centroid);
                }
                cameraPivot /= selectedGroup.Count;
            } else if (Input.GetKey(KeyCode.F)) {
                cameraMovement.LookAt(cameraPivot);
            } else if (Input.GetMouseButtonDown(1)) { // right click cancel selection
                SwapSelect(null);
            }
        }
        if (hasSelectedGroup) { // single group selection mode
            if (hoverObj != null && Input.GetMouseButtonDown(0)) { // left click on object
                if (selectedGroup.Contains(hoverObj)) { // in group already, remove from group
                    selectedGroup.Remove(hoverObj);
                    HashSet<ObjectSelector> newGroup = new HashSet<ObjectSelector>();
                    newGroup.Add(hoverObj);
                    string newName = hoverObj.gameObject.name;
                    if (groupNameToObjs.ContainsKey(newName)) {
                        newName += "_";
                    }
                    objToGroupName[hoverObj] = newName;
                    groupNameToObjs[newName] = newGroup;
                } else { // add to group
                    string currName = objToGroupName[hoverObj];
                    HashSet<ObjectSelector> currGroup = groupNameToObjs[currName];
                    if (currGroup.Count == 1) {
                        groupNameToObjs.Remove(currName);
                    } else {
                        currGroup.Remove(hoverObj);
                    }
                    selectedGroup.Add(hoverObj);
                    objToGroupName[hoverObj] = selectedGroupName;
                }
                hoverObj.UpdateColor();
            } else if (Input.GetKey(KeyCode.R)) { // rename group
                labelInputField.ActivateInputField();
            } else if (Input.GetKey(KeyCode.F)) {
                cameraMovement.LookAt(cameraPivot);
            } else if (Input.GetMouseButtonDown(1)) { // right click
                SwapSelect(null);
            }
        } else {
            if (hoverObj != null && Input.GetMouseButtonDown(0)) { // select an object
                SwapSelect(hoverObj);
                cameraPivot = selectedObj.gameObject.transform.TransformPoint(selectedObj.centroid);
                labelInputField.text = selectedGroupName;
            }
        }
        // camera movements
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        cameraMovement.ScrollTranslate(scroll, Vector3.forward);
        if (IsControlPressed()) {
            if (Input.GetKey(KeyCode.UpArrow)) {
                cameraMovement.KeyTranslate(Vector3.up);
            }
            if (Input.GetKey(KeyCode.DownArrow)) {
                cameraMovement.KeyTranslate(-Vector3.up);
            }
            if (Input.GetKey(KeyCode.RightArrow)) {
                cameraMovement.KeyTranslate(Vector3.right);
            }
            if (Input.GetKey(KeyCode.LeftArrow)) {
                cameraMovement.KeyTranslate(-Vector3.right);
            }
        } else {
            bool selfPivot = (selectedObj == null && !hasSelectedGroup);
            if (Input.GetKey(KeyCode.UpArrow)) {
                cameraMovement.RotateAround(cameraPivot, Vector3.right, true, selfPivot);
            }
            if (Input.GetKey(KeyCode.DownArrow)) {
                cameraMovement.RotateAround(cameraPivot, Vector3.right, false, selfPivot);
            }
            if (Input.GetKey(KeyCode.Period)) {
                cameraMovement.RotateAround(cameraPivot, Vector3.up, true, selfPivot);
            }
            if (Input.GetKey(KeyCode.Comma)) {
                cameraMovement.RotateAround(cameraPivot, Vector3.up, false, selfPivot);
            }
            if (Input.GetKey(KeyCode.RightArrow)) {
                cameraMovement.RotateAround(cameraPivot, Vector3.forward, true, selfPivot);
            }
            if (Input.GetKey(KeyCode.LeftArrow)) {
                cameraMovement.RotateAround(cameraPivot, Vector3.forward, false, selfPivot);
            }
        }

    }
}
