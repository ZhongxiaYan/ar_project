using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Manager : MonoBehaviour {
    List<GameObject> orderList;
    Dictionary<GameObject, Vector3> origCentroid;
    Dictionary<GameObject, Vector3> newCentroid;
    Vector3 baseOffset = new Vector3();
    bool isBaseSet = false;

	void Start () {
        orderList = new List<GameObject>();
        origCentroid = new Dictionary<GameObject, Vector3>();
        newCentroid = new Dictionary<GameObject, Vector3>();

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

            origCentroid[child] = centroid;
            newCentroid[child] = 2 * centroid;
            child.AddComponent<MeshCollider>();
            child.AddComponent<MouseEvent>().manager = this;
        }
	}

    public void AddToQueue(GameObject child) {
        orderList.Add(child);
        if (!isBaseSet) {
            baseOffset = newCentroid[child] - origCentroid[child];
            isBaseSet = true;
            return;
        }
        Vector3 offset = origCentroid[child] - newCentroid[child] + baseOffset;
        Mesh mesh = child.GetComponent<MeshFilter>().mesh;
        Vector3[] vertices = mesh.vertices;
        int i = 0;
        foreach (Vector3 vertex in vertices) {
            vertices[i] += offset;
            i++;
        }
        mesh.vertices = vertices;
        mesh.RecalculateBounds();
    }

}
