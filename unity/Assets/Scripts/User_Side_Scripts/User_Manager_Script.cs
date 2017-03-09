using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using System.Text;
using System.IO;

public class User_Manager_Script : MonoBehaviour
{

    //original offset from total centroid
    //Dictionary<string, Vector3> newDisplacement;
    //Dictionary<string, Vector3> oldDispFromCentroid;
    Dictionary<string, GameObject> objectList;
    Dictionary<string, string> tableInfo;
    Dictionary<string, Vector3> newDisp;
    Dictionary<string, Vector3> oldDisp;

    Vector3 totalCentroid;
    List<string> orderlist;
    int partCount;
    int currentIndex;
    public enum MoveType { Time, Speed }
    public static MoveObject use = null;
    public Text titleText;
    public 

    // Use this for initialization
    void Start()
    {

        GameObject obj = OBJLoader.LoadOBJFile("Assets/LEGO_CAR_B1_small.obj");

        obj.transform.position = new Vector3(0, 0, 0);
        newDisp = new Dictionary<string, Vector3>();
        oldDisp = new Dictionary<string, Vector3>();
        //oldDispFromCentroid = new Dictionary<string, Vector3>();
        objectList = new Dictionary<string, GameObject>();
        orderlist = new List<string>();
        tableInfo = new Dictionary<string, string>();

        //trial code to set the orderlist.
        //orderlist = new List<string>();
        //orderlist.Add("5_5");
        //orderlist.Add("6_6");
        //orderlist.Add("7_7");
        //orderlist.Add("8_8");
        //orderlist.Add("1_1");
        //orderlist.Add("2_2");
        //orderlist.Add("3_3");
        //orderlist.Add("4_4");
        //orderlist.Add("9_9");
        //orderlist.Add("10_10");

        loadDataFromFile("sequence");

        //index of the current thing
        currentIndex = 1;

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

            oldDisp[child.name] = child.transform.position;

            Vector3 currentcentroid = calcCentroid(vertices);
            Debug.Log(child.name);
            Debug.Log(currentcentroid + "current");
            Debug.Log(totalCentroid + "total");
            Vector3 off = currentcentroid - totalCentroid;
            off = new Vector3(off.x, off.y * -1, off.z * -1) * -1;
            Debug.Log(off + "offset");
            int i = 0;
            newDisp[child.name] = off * 2;
            StartCoroutine(TranslateTo(child.transform, newDisp[child.name], 2, MoveType.Time));

            //Now you can move this however you want and the
            //replace method will always bring them back to
            //center view.
            //I'm not using a base part though. Hmm
            //maybe I should do that.
        }
    }
 
    private bool loadDataFromFile(string fileName)
    {
        // Handle any problems that might arise when reading the text
        try
        {
            string line;
            // Create a new StreamReader, tell it which file to read and what encoding the file
            // was saved as
            Debug.Log("begin the try");
            FileInfo theSourceFile = new FileInfo("Assets/sequence.out");
            StreamReader theReader = theSourceFile.OpenText();
            // Immediately clean up the reader after this block of code is done.
            // You generally use the "using" statement for potentially memory-intensive objects
            // instead of relying on garbage collection.
            // (Do not confuse this with the using directive for namespace at the 
            // beginning of a class!)


            using (theReader)
            {
                // While there's lines left in the text file, do this:
                do
                {
                    line = theReader.ReadLine();
                    Debug.Log("line " + line);

                    if (line != null)
                    {
                        // Do whatever you need to do with the text line, it's a string now
                        // In this example, I split it into arguments based on comma
                        // deliniators, then send that array to DoStuff()
                        
                        string[] entries = line.Split(',');
                        Debug.Log("entries" + entries[0]);
                        if (entries.Length == 2)
                        {
                            tableInfo[entries[0]] = entries[1];
                            orderlist.Add(entries[0]);
                        } 
                    }
                }
                while (line != null);
                // Done reading, close the reader and return true to broadcast success    
                theReader.Close();
                return true;
            }
        }
        // If anything broke in the try block, we throw an exception with information
        // on what didn't work
        catch (System.Exception e)
        {
            Debug.Log("ERROR: File not found");
            return false;
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
        moveNextObject();
        updateTextFields();
    }

    public void moveNextObject()
    {
        //get the next object to move from the orderlist at the current index
        if (currentIndex < partCount - 1)
        {
            if (orderlist[currentIndex] != null)
            {
                GameObject prevchild = objectList[orderlist[currentIndex]];
                prevchild.transform.position = oldDisp[prevchild.name];
            }
            currentIndex++;
        }
        Debug.Log("moving" + currentIndex);
        GameObject child = objectList[orderlist[currentIndex]];
        child.transform.position = newDisp[child.name];
        StartCoroutine(TranslateTo(child.transform, oldDisp[child.name], 1, MoveType.Time));
    }

    public void prevButtonOnClick()
    {
        Debug.Log("PrevButtonClicked");
        movePrevObject();
        updateTextFields();
    }

    //not done yet
    public void movePrevObject()
    {
        //get the next object to move from the orderlist at the current index
        if (currentIndex > 0)
        {

            GameObject prevchild = objectList[orderlist[currentIndex]];
            prevchild.transform.position = newDisp[prevchild.name];
            currentIndex--;
        }
        Debug.Log("moving" + currentIndex);
        GameObject child = objectList[orderlist[currentIndex]];
        child.transform.position = oldDisp[child.name];
        StartCoroutine(TranslateTo(child.transform, newDisp[child.name], 1, MoveType.Time));
    }

    public void replayButtonOnClick()
    {
        Debug.Log("Replay Button Clicked");
        GameObject child = objectList[orderlist[currentIndex]];
        child.transform.position = newDisp[child.name];
        StartCoroutine(TranslateTo(child.transform, new Vector3(0,0,0), 1, MoveType.Time));
    }

    public void updateTextFields()
    {
        titleText.text = "Step " + (currentIndex+1);
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
