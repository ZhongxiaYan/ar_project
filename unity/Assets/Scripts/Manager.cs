using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Manager : MonoBehaviour {
    List<ObjectSelector> orderList;
    Dictionary<ObjectSelector, Vector3> origCentroid;
    Vector3 baseOffset;
    bool isBaseSet = false;

	void Start() {
        cameraMovement = GameObject.Find("Main Camera").GetComponent<CameraMovement>();
        labelInputField = GameObject.Find("Canvas/LabelInputField").GetComponent<InputField>();
        orderList = new List<ObjectSelector>();
        origCentroid = new Dictionary<ObjectSelector, Vector3>();

        GameObject obj = OBJLoader.LoadOBJFile("Assets/LEGO_CAR_B1_small.obj");

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
        }

        cameraMovement.LookAtZoom(children);
	}

    public void AddToQueue(ObjectSelector objSelector) {
        orderList.Add(objSelector);
        if (!isBaseSet) {
            baseOffset = objSelector.centroid - origCentroid[objSelector];
            isBaseSet = true;
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
    public ObjectSelector renamingChildSelector = null;
    InputField labelInputField = null;

    bool IsTextFieldFocused() {
        return labelInputField.isFocused;
    }

    public void HandleRename(string name) {
        renamingChildSelector.gameObject.name = name;
        renamingChildSelector = null;
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
        if (pivotChildSelector != null && Input.GetMouseButtonDown(1)) {
            SwapPivot(null);
        }
        if (renamingChildSelector == null) { // set hover text only if no child is being renamed
            if (hoverChildSelector != null) {
                labelInputField.text = hoverChildSelector.gameObject.name;
            } else {
                labelInputField.text = "";
            }
        }
        // process keyboard inputs
        if (IsTextFieldFocused()) { // we're typing in a text field, ignore any keyboard inputs
            return;
        }
        if (hoverChildSelector != null) { // mouse is hovering over this child
            if (Input.GetKeyUp(KeyCode.P)) { // assigning a pivot to the hovered child
                SwapPivot(hoverChildSelector);
                cameraMovement.LookAt(pivotChildSelector.gameObject.transform.TransformPoint(pivotChildSelector.centroid));
            } else if (Input.GetKeyUp(KeyCode.R)) { // renaming hovered child
                renamingChildSelector = hoverChildSelector;
                labelInputField.text = renamingChildSelector.gameObject.name;
                labelInputField.Select();
            }
        }
        // camera movements
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        cameraMovement.ScrollTranslate(scroll, Vector3.forward);
        if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) {
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
