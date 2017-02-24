using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Manager : MonoBehaviour {
    List<ObjectSelector> orderList;
    Dictionary<ObjectSelector, Vector3> origCentroid;
    Vector3 baseOffset;
    bool isBaseSet = false;
    public bool isTextFieldSelected = false;
    public ObjectSelector selectedChildSelector = null;
    public ObjectSelector renamingChildSelector = null;
    InputField labelInputField = null;

	void Start() {
        labelInputField = GameObject.Find("Canvas/LabelInputField").GetComponent<InputField>();
        orderList = new List<ObjectSelector>();
        origCentroid = new Dictionary<ObjectSelector, Vector3>();

        GameObject obj = OBJLoader.LoadOBJFile("Assets/LEGO_CAR_B1_small.obj");

        foreach (Transform childTrans in obj.transform) {
            GameObject child = childTrans.gameObject;
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
            childObjSelector.manager = this;
            childObjSelector.centroid = newCentroid;
        }
	}

    public void SetSelected(ObjectSelector objSelector) {
        ObjectSelector oldSelector = selectedChildSelector;
        selectedChildSelector = objSelector;
        if (oldSelector != null) {
            oldSelector.UpdateColor();
        }
    }

    public void HoverEnter(ObjectSelector objSelector) {
        if (renamingChildSelector != null) { // another object is still being renamed
            return;
        }
        labelInputField.text = objSelector.gameObject.name;
    }

    public void SetRenaming(ObjectSelector objSelector) {
        if (renamingChildSelector != null) { // another object is still being renamed
            return;
        }
        renamingChildSelector = objSelector;
        labelInputField.text = renamingChildSelector.gameObject.name;
        isTextFieldSelected = true;
        labelInputField.Select();
    }

    public void AddToQueue(ObjectSelector objSelector) {
        if (objSelector == selectedChildSelector) {
            selectedChildSelector = null;
        }
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

    void Update() {
        if (selectedChildSelector != null && Input.GetMouseButtonDown(1)) {
            SetSelected(null);
        }
    }

    public void HandleRename(string name) {
        renamingChildSelector.gameObject.name = name;
        renamingChildSelector = null;
        isTextFieldSelected = false;
    }

}
