using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class User_Manager_Script : MonoBehaviour {

    //original offset from total centroid
    Dictionary<string, Vector3> origOffsetFromTotalCentroid;
    Dictionary<string, Vector3> newOffsetFromTotalCentroid;
    Dictionary<string, GameObject> objectList;
    Dictionary<GameObject, Vector3> newCentroid;
    Vector3 totalCentroid;
    List<string> orderlist;
    int partCount;
    int currentIndex;

    // Use this for initialization
    void Start () {
        GameObject obj = OBJLoader.LoadOBJFile("Assets/LEGO_CAR_B1_small.obj");
        obj.transform.position = new Vector3(0, 0, 0);

        origOffsetFromTotalCentroid = new Dictionary<string, Vector3>();
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

        totalCentroid = new Vector3();
        partCount = 0;
        foreach (Transform childTrans in obj.transform)
        {
            GameObject child = childTrans.gameObject;
            Mesh mesh = child.GetComponent<MeshFilter>().mesh;
            Vector3[] vertices = mesh.vertices;

            Vector3 currentcentroid = calcCentroid(vertices);
            totalCentroid += currentcentroid;
            partCount++;

            objectList[child.name] = child;
        }
        totalCentroid /= partCount;

        foreach (Transform childTrans in obj.transform)
        {
            GameObject child = childTrans.gameObject;
            Mesh mesh = child.GetComponent<MeshFilter>().mesh;
            Vector3[] vertices = mesh.vertices;

            Vector3 currentcentroid = calcCentroid(vertices);
            Vector3 offsetFromTotalCentroid = currentcentroid - totalCentroid;
            origOffsetFromTotalCentroid[child.name] = offsetFromTotalCentroid;
            
            int i = 0;

            //Now you can move this however you want and the
            //replace method will always bring them back to
            //center view.
            //I'm not using a base part though. Hmm
            //maybe I should do that.
            foreach (Vector3 vertex in vertices)
            {
                vertices[i] += 2*offsetFromTotalCentroid;
                i++;
            }
            mesh.vertices = vertices;
            mesh.RecalculateBounds();

            
        }
    }

    private Vector3 calcCentroid(Vector3[] points)
    {
        //calc centroid for each vertex
        Vector3 centroid = new Vector3();
        foreach (Vector3 vertex in points)
        {
            centroid += vertex;
        }
        centroid /= points.Length;
        return centroid;
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
        Vector3 currentcentroid = calcCentroid(vertices);

        int i = 0;
        foreach (Vector3 vertex in vertices)
        {
            vertices[i] = vertices[i] - currentcentroid + origOffsetFromTotalCentroid[child.name] + totalCentroid;
            i++;
        }
        mesh.vertices = vertices;
        mesh.RecalculateBounds();

        currentIndex++;
    }

    public void prevButtonOnClick()
    {
        Debug.Log("PrevButtonClicked", gameObject);
        movePrevObject();
    }

    //not done yet
    public void movePrevObject()
    {
        //get the next object to move from the orderlist at the current index
        Debug.Log("moving" + currentIndex);
        GameObject child = objectList[orderlist[currentIndex]];
        Mesh mesh = child.GetComponent<MeshFilter>().mesh;
        Vector3[] vertices = mesh.vertices;
        Vector3 currentcentroid = calcCentroid(vertices);

        int i = 0;
        foreach (Vector3 vertex in vertices)
        {
            vertices[i] = vertices[i] - currentcentroid + origOffsetFromTotalCentroid[child.name] + totalCentroid;
            i++;
        }
        mesh.vertices = vertices;
        mesh.RecalculateBounds();

        currentIndex++;
    }

    // Update is called once per frame
    void Update () {
		
	}

    /**
       //expand
       foreach (Transform childTrans in obj.transform)
       {
           //get each child
           GameObject child = childTrans.gameObject;
           Mesh mesh = child.GetComponent<MeshFilter>().mesh;
           Vector3[] vertices = mesh.vertices;
           Vector3 childcentroid = new Vector3();

           //calc centroid for each vertex
           foreach (Vector3 vertex in vertices)
           {
               childcentroid += vertex;
           }
           childcentroid /= vertices.Length;

           
           //add childcentroid to each vertex
           //MODIFY THIS
           //the original points are too far away.
           int i = 0;
           foreach (Vector3 vertex in vertices)
           {
               //Debug.Log(childcentroid);
               //Debug.Log(vertices[i] + childcentroid);
               vertices[i] += childcentroid;
               i++;
           }
           mesh.vertices = vertices;
           mesh.RecalculateBounds();
           Vector3 newCentroid = childcentroid + childcentroid;
           Debug.Log(child.name);
       }
       **/
}
