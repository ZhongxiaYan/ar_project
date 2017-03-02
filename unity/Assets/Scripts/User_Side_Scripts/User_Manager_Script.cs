using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class User_Manager_Script : MonoBehaviour
{

    //original offset from total centroid
    Dictionary<string, Vector3> newDisplacement;
    //Dictionary<string, Vector3> oldDispFromCentroid;
    Dictionary<string, GameObject> objectList;
    Dictionary<GameObject, Vector3> newCentroid;
    Vector3 totalCentroid;
    List<string> orderlist;
    int partCount;
    int currentIndex;
    public enum MoveType { Time, Speed }
    public static MoveObject use = null;

    // Use this for initialization
    void Start () {

        GameObject obj = OBJLoader.LoadOBJFile("Assets/LEGO_CAR_B1_small.obj");

        obj.transform.position = new Vector3(0, 0, 0);
        newDisplacement = new Dictionary<string, Vector3>();
        //oldDispFromCentroid = new Dictionary<string, Vector3>();
        objectList = new Dictionary<string, GameObject>();

        //trial code to set the orderlist.
        orderlist = new List<string>();
        orderlist.Add("5_5");
        orderlist.Add("6_6");
        orderlist.Add("7_7");
        orderlist.Add("8_8");
        orderlist.Add("1_1");
        orderlist.Add("2_2");
        orderlist.Add("3_3");
        orderlist.Add("4_4");
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

            Debug.Log("vertices" + vertices.Length);
            objectList[child.name] = child;
        }
        totalCentroid /= partCount;

        GameObject parentObject = new GameObject();
        //parentObject.transform.Rotate(new Vector3(20, 0, 0));
        parentObject.transform.Translate(totalCentroid);
        obj.transform.SetParent(parentObject.transform);

        //move each object to new positions
        foreach (Transform childTrans in obj.transform)
        {
            GameObject child = childTrans.gameObject;
            Mesh mesh = child.GetComponent<MeshFilter>().mesh;
            Vector3[] vertices = mesh.vertices;

            Vector3 currentcentroid = calcCentroid(vertices);
            Debug.Log(child.name);
            Debug.Log(currentcentroid + "current");
            Debug.Log(totalCentroid + "total");
            Vector3 off = currentcentroid - totalCentroid;
            off = new Vector3(off.x, off.y * -1, off.z * -1);
            Debug.Log(off + "offset");
            int i = 0;
            StartCoroutine(TranslateTo(child.transform, off*-2, 2, MoveType.Time));

            //Now you can move this however you want and the
            //replace method will always bring them back to
            //center view.
            //I'm not using a base part though. Hmm
            //maybe I should do that.
            foreach (Vector3 vertex in vertices)
            {
                vertices[i] += 2*off;
                i++;
            }
            newDisplacement[child.name] = calcCentroid(vertices) - totalCentroid;
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
        Debug.Log("NextButtonClicked");
        if (currentIndex < partCount)
        {
            moveNextObject();
        }
    }

    public void moveNextObject()
    {
        if (currentIndex < partCount-1)
        {
            currentIndex++;
        }
        //get the next object to move from the orderlist at the current index
        Debug.Log("moving" + currentIndex);
        GameObject child = objectList[orderlist[currentIndex]];
        StartCoroutine(TranslateTo(child.transform, new Vector3(0, 0, 0), 1, MoveType.Time));
    }

    public void prevButtonOnClick()
    {
        Debug.Log("PrevButtonClicked");
        if (currentIndex >= 0)
        {
            movePrevObject();
        }
    }

    //not done yet
    public void movePrevObject()
    {
        if(currentIndex > 0)
        {
            currentIndex--;
        }
        //get the next object to move from the orderlist at the current index
        Debug.Log("moving" + currentIndex);
        GameObject child = objectList[orderlist[currentIndex]];
        StartCoroutine(TranslateTo(child.transform, new Vector3(2, 2, 2), 1, MoveType.Time));
    }

    public IEnumerator TranslateTo(Transform thisTransform, Vector3 endPos, float value, MoveType moveType)
    {
        yield return Translation(thisTransform, thisTransform.position, endPos, value, moveType);
    }

    public IEnumerator Translation(Transform thisTransform, Vector3 endPos, float value, MoveType moveType)
    {
        yield return Translation(thisTransform, thisTransform.position, thisTransform.position + endPos, value, moveType);
    }

    public IEnumerator Translation(Transform thisTransform, Vector3 startPos, Vector3 endPos, float value, MoveType moveType)
    {
        float rate = (moveType == MoveType.Time) ? 1.0f / value : 1.0f / Vector3.Distance(startPos, endPos) * value;
        float t = 0.0f;
        while (t < 1.0)
        {
            t += Time.deltaTime * rate;
            thisTransform.position = Vector3.Lerp(startPos, endPos, Mathf.SmoothStep(0.0f, 1.0f, t));
            yield return null;
        }
    }

    public IEnumerator Rotation(Transform thisTransform, Vector3 degrees, float time)
    {
        Quaternion startRotation = thisTransform.rotation;
        Quaternion endRotation = thisTransform.rotation * Quaternion.Euler(degrees);
        float rate = 1.0f / time;
        float t = 0.0f;
        while (t < 1.0f)
        {
            t += Time.deltaTime * rate;
            thisTransform.rotation = Quaternion.Slerp(startRotation, endRotation, t);
            yield return null;
        }
    }

    // Update is called once per frame
    void Update () {
		
	}
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

/**
 * Mesh mesh = child.GetComponent<MeshFilter>().mesh;
    Vector3[] vertices = mesh.vertices;
    Vector3 currentcentroid = calcCentroid(vertices);

    int i = 0;
    foreach (Vector3 vertex in vertices)
    {
        vertices[i] = vertices[i] - currentcentroid + origDisplacement[child.name] + totalCentroid;
        i++;
    }
    mesh.vertices = vertices;
    mesh.RecalculateBounds();
    **/

/**
    Mesh mesh = child.GetComponent<MeshFilter>().mesh;
    Vector3[] vertices = mesh.vertices;
    Vector3 currentcentroid = calcCentroid(vertices);

    int i = 0;
    foreach (Vector3 vertex in vertices)
    {
        vertices[i] = vertices[i] - currentcentroid + newOffsetFromTotalCentroid[child.name] + totalCentroid;
        i++;
    }
    mesh.vertices = vertices;
    mesh.RecalculateBounds();
    
    currentIndex--;
    **/
