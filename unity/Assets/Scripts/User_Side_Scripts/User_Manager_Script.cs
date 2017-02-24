using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class User_Manager_Script : MonoBehaviour {

    Dictionary<string, Vector3> origCentroid;
    Dictionary<string, GameObject> objectList;
    Dictionary<GameObject, Vector3> newCentroid;
    List<string> orderlist;
    int currentIndex;

    // Use this for initialization
    void Start () {
        GameObject obj = OBJLoader.LoadOBJFile("Assets/LEGO_CAR_B1_small.obj");
        obj.transform.position = new Vector3(0, 0, 0);

        origCentroid = new Dictionary<string, Vector3>();
        objectList = new Dictionary<string, GameObject>();

        //trial code to set the orderlist.
        orderlist = new List<string>();
        orderlist.Add("1_1");
        orderlist.Add("2_2");
        orderlist.Add("3_3");
        orderlist.Add("4_4");
        orderlist.Add("5_5");
        orderlist.Add("6_6");
        orderlist.Add("7_7");
        orderlist.Add("8_8");
        orderlist.Add("9_9");
        orderlist.Add("10_10");

        //index of the current thing
        currentIndex = 0;

        //expand
        foreach (Transform childTrans in obj.transform)
        {
            //get each child
            GameObject child = childTrans.gameObject;
            Mesh mesh = child.GetComponent<MeshFilter>().mesh;
            Vector3[] vertices = mesh.vertices;
            Vector3 childcentroid = new Vector3();
            foreach (Vector3 vertex in vertices)
            {
                childcentroid += vertex;
            }
            childcentroid /= vertices.Length;

            int i = 0;
            foreach (Vector3 vertex in vertices)
            {
                vertices[i] += childcentroid;
                i++;
            }
            mesh.vertices = vertices;
            mesh.RecalculateBounds();
            Vector3 newCentroid = childcentroid + childcentroid;
            origCentroid[child.name] = childcentroid;
            objectList[child.name] = child;
            Debug.Log(child.name);
        }
    }

    public void nextButtonOnClick()
    {
        Debug.Log("NextButtonClicked", gameObject);
        moveNextObject();
    }

    public void moveNextObject()
    {
        //get the next object to move from the orderlist at the current index
        Debug.Log("moving" + currentIndex);
        GameObject child = objectList[orderlist[currentIndex]];
        Mesh mesh = child.GetComponent<MeshFilter>().mesh;
        Vector3[] vertices = mesh.vertices;
        Vector3 centroid = new Vector3();
        foreach (Vector3 vertex in vertices)
        {
            centroid += vertex;
        }
        centroid /= vertices.Length;
        currentIndex++;
    }

    public void prevButtonOnClick()
    {
        Debug.Log("PrevButtonClicked", gameObject);
    }

    // Update is called once per frame
    void Update () {
		
	}
}
