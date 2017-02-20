using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Manager : MonoBehaviour {
    List<GameObject> orderList;
    Dictionary<GameObject, Vector3> origCentroid;
    Vector3 baseOffset = new Vector3();
    bool isBaseSet = false;
    public ObjectSelector selectedChildSelector = null;

	void Start() {
        orderList = new List<GameObject>();
        origCentroid = new Dictionary<GameObject, Vector3>();

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

            origCentroid[child] = centroid;
            child.AddComponent<MeshCollider>();
            ObjectSelector childObjSelector = child.AddComponent<ObjectSelector>();
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

    public void AddToQueue(ObjectSelector objSelector) {
        if (objSelector == selectedChildSelector) {
            selectedChildSelector = null;
        }
        GameObject child = objSelector.gameObject;
        orderList.Add(child);
        if (!isBaseSet) {
            baseOffset = objSelector.centroid - origCentroid[child];
            isBaseSet = true;
            return;
        }
        Vector3 offset = origCentroid[child] - objSelector.centroid + baseOffset;
        Vector3 newCentroid = objSelector.centroid + offset;
        Mesh mesh = child.GetComponent<MeshFilter>().mesh;
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

}
