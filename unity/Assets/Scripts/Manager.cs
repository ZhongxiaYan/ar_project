﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

public class Manager : MonoBehaviour {
    List<ObjectSelector> orderList;
    HashSet<ObjectSelector> queueSet;
    Dictionary<ObjectSelector, string> newNames;
    Dictionary<string, ObjectSelector> nameToObject;
    Dictionary<ObjectSelector, Vector3> origCentroid;
    Vector3 baseOffset;
    readonly string GROUP_SUFFIX = "_parts.csv";

	void Start() {
        cameraMovement = GameObject.Find("Main Camera").GetComponent<CameraMovement>();
        labelInputField = GameObject.Find("Canvas/LabelInputField").GetComponent<InputField>();
        orderList = new List<ObjectSelector>();
        queueSet = new HashSet<ObjectSelector>();
        newNames = new Dictionary<ObjectSelector, string>();
        origCentroid = new Dictionary<ObjectSelector, Vector3>();
        renameChildSelectors = new HashSet<ObjectSelector>();

        // load("Assets/LEGO_CAR_B1_small.obj");
	}

    void load(string path) {
        GameObject obj = OBJLoader.LoadOBJFile(path);

        List<GameObject> children = new List<GameObject>();
        foreach (Transform childTrans in obj.transform) {
            children.Add(childTrans.gameObject);
        }

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
            origCentroid[childObjSelector] = centroid;
            childObjSelector.centroid = newCentroid;
            newNames[childObjSelector] = child.name;
            nameToObject[child.name] = childObjSelector;
        }

        using (System.IO.StreamReader file = new System.IO.StreamReader(path + GROUP_SUFFIX)) {
            while (!file.EndOfStream) {
                string[] splits = file.ReadLine().Split(',');

            }
        }

        cameraMovement.LookAtZoom(children);
    }

    public void LeftClick(ObjectSelector objSelector) {
        if (renameChildSelectors.Count > 0 && IsControlPressed()) {
            renameChildSelectors.Add(objSelector);
            objSelector.UpdateColor();
            return;
        } else if (renameChildSelectors.Count > 0) { // don't add to queue if something was just being renamed
            return;
        } else if (queueSet.Contains(objSelector)) { // already in queue
            return;
        }
        // add object to queue
        orderList.Add(objSelector);
        queueSet.Add(objSelector);
        objSelector.UpdateColor();
        if (orderList.Count == 1) { // first one, set base
            baseOffset = objSelector.centroid - origCentroid[objSelector];
            return;
        }
        Vector3 offset = origCentroid[objSelector] - objSelector.centroid + baseOffset;
        Vector3 newCentroid = objSelector.centroid + offset;
        Mesh mesh = objSelector.gameObject.GetComponent<MeshFilter>().mesh;
        Vector3[] vertices = mesh.vertices;
        int i = 0;
        foreach (Vector3 vertex in vertices) {
            vertices[i] += offset;
            i++;
        }
        mesh.vertices = vertices;
        mesh.RecalculateBounds();
        objSelector.centroid = newCentroid;
    }

    CameraMovement cameraMovement = null;
    public ObjectSelector hoverChildSelector = null;
    public ObjectSelector pivotChildSelector = null;
    public HashSet<ObjectSelector> renameChildSelectors = null;
    InputField labelInputField = null;

    bool IsTextFieldFocused() {
        return labelInputField.isFocused;
    }

    bool IsControlPressed() {
        return Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
    }

    public void HandleRename(string name) {
        if (!IsControlPressed()) {
            HashSet<ObjectSelector> renameSelectorsCopy = new HashSet<ObjectSelector>(renameChildSelectors);
            renameChildSelectors.Clear();
            foreach (ObjectSelector selector in renameSelectorsCopy) {
                newNames[selector] = name;
                selector.UpdateColor();
            }
            labelInputField.DeactivateInputField();
        } else {
            labelInputField.ActivateInputField();
        }
    }

    public void HandleLoad() {
        try {
            string s = EditorUtility.OpenFilePanel("Select Input File", "./", "");
            System.Diagnostics.Process process = new System.Diagnostics.Process();
            System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
            startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            startInfo.FileName = "cmd.exe";
            startInfo.Arguments = System.String.Format("/C python Assets/Python/group_parts.py {0} {0}{1}", s, GROUP_SUFFIX);
            process.StartInfo = startInfo;
            process.Start();
            process.WaitForExit();
            load(s);
        } catch (System.ArgumentException ae) {

        }
    }

    public void HandleExport() {
        string path = EditorUtility.SaveFilePanel("Enter Output Path", "./", "sequence", "out");
        using (System.IO.StreamWriter file = new System.IO.StreamWriter(path)) {
            foreach (ObjectSelector selector in orderList) {
                file.WriteLine(selector.gameObject.name + "," + newNames[selector]);
            }
        }
    }

    void SwapPivot(ObjectSelector newPivot) {
        ObjectSelector oldPivotSelector = pivotChildSelector;
        pivotChildSelector = newPivot;
        if (pivotChildSelector != null) {
            pivotChildSelector.UpdateColor();
        }
        if (oldPivotSelector != null) {
            oldPivotSelector.UpdateColor();
        }
    }

    void LateUpdate() {
        if (pivotChildSelector != null && Input.GetMouseButtonDown(1)) { // right click cancels the selection
            SwapPivot(null);
        }
        if (renameChildSelectors.Count == 0) { // set hover text only if no child is being renamed
            if (hoverChildSelector != null) {
                labelInputField.text = newNames[hoverChildSelector];
            } else {
                labelInputField.text = "";
            }
        }
        // process keyboard inputs
        if (IsTextFieldFocused()) { // we're typing in a text field, ignore any keyboard inputs
            return;
        }
        if (hoverChildSelector != null) { // mouse is hovering over this child
            if (Input.GetKey(KeyCode.P)) { // assigning a pivot to the hovered child
                SwapPivot(hoverChildSelector);
                cameraMovement.LookAt(pivotChildSelector.gameObject.transform.TransformPoint(pivotChildSelector.centroid));
            } else if (pivotChildSelector == null && Input.GetKey(KeyCode.R)) { // renaming hovered child
                renameChildSelectors.Add(hoverChildSelector);
                hoverChildSelector.UpdateColor();
                labelInputField.text = hoverChildSelector.gameObject.name;
                labelInputField.ActivateInputField();
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
            Vector3 pivot = new Vector3(0, 0, 0);
            bool selfPivot = (pivotChildSelector == null);
            if (!selfPivot) {
                pivot = pivotChildSelector.gameObject.transform.TransformPoint(pivotChildSelector.centroid);
            }
            if (Input.GetKey(KeyCode.UpArrow)) {
                cameraMovement.RotateAround(pivot, Vector3.right, true, selfPivot);
            }
            if (Input.GetKey(KeyCode.DownArrow)) {
                cameraMovement.RotateAround(pivot, Vector3.right, false, selfPivot);
            }
            if (Input.GetKey(KeyCode.Period)) {
                cameraMovement.RotateAround(pivot, Vector3.up, true, selfPivot);
            }
            if (Input.GetKey(KeyCode.Comma)) {
                cameraMovement.RotateAround(pivot, Vector3.up, false, selfPivot);
            }
            if (Input.GetKey(KeyCode.RightArrow)) {
                cameraMovement.RotateAround(pivot, Vector3.forward, true, selfPivot);
            }
            if (Input.GetKey(KeyCode.LeftArrow)) {
                cameraMovement.RotateAround(pivot, Vector3.forward, false, selfPivot);
            }
        }

    }
}
