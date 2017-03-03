using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour {
    float zoomSpeed = 200f;
    float translateSpeed = 100f;
    float turnSpeed = 100f;

    Camera cameraComponent = null;

    void Start() {
        cameraComponent = gameObject.GetComponent<Camera>();
    }

    public void ScrollTranslate(float scroll, Vector3 v) {
        transform.Translate(zoomSpeed * Time.deltaTime * scroll * v);
    }

    public void KeyTranslate(Vector3 v) {
        transform.Translate(translateSpeed * Time.deltaTime * v);
    }

    public void RotateAround(Vector3 pivot, Vector3 axis, bool ccw, bool self) {
        if (self) {
            transform.Rotate(axis, (ccw ? 1 : -1) * turnSpeed * Time.deltaTime);
        } else {
            transform.RotateAround(pivot, axis, (ccw ? 1 : -1) * turnSpeed * Time.deltaTime);
        }
    }

    public void LookAt(Vector3 point) {
        transform.LookAt(point);
    }

    public void LookAtZoom(List<GameObject> objects) {
        Vector3 centroid = new Vector3();
        List<Vector3> centroids = new List<Vector3>();
        List<float> radii = new List<float>();
        // find average of the centroids of all objects
        foreach (GameObject obj in objects) {
            Vector3[] vertices = obj.GetComponent<MeshFilter>().mesh.vertices;
            Vector3 objCentroid = new Vector3();
            foreach (Vector3 vertex in vertices) {
                objCentroid += vertex;
            }
            objCentroid /= vertices.Length;
            Vector3 objCentroidWorld = obj.transform.TransformPoint(objCentroid);
            centroid += objCentroidWorld;
            float maxDist = 0;
            Vector3 farthestVertex = new Vector3();
            foreach (Vector3 vertex in vertices) {
                float dist = Vector3.Distance(objCentroid, vertex);
                if (dist > maxDist) {
                    maxDist = dist;
                    farthestVertex = vertex;
                }
            }
            centroids.Add(objCentroidWorld);
            radii.Add(Vector3.Distance(obj.transform.TransformPoint(farthestVertex), objCentroidWorld));
        }
        centroid /= objects.Count;
        transform.LookAt(centroid);
        float nearClipPlaneDist = cameraComponent.nearClipPlane;
        float nearClipPlaneUpDist = nearClipPlaneDist * Mathf.Tan(cameraComponent.fieldOfView * 0.5f * Mathf.Deg2Rad);
        float nearClipPlaneRightDist = nearClipPlaneUpDist * cameraComponent.aspect;
        float moveForwardDist = float.PositiveInfinity;
        for (int i = 0; i < centroids.Count; i++) {
            Vector3 objCentroid = centroids[i];
            float radius = radii[i];
            Vector3 vec = objCentroid - transform.position;
            float forwardDisp = Vector3.Dot(vec, transform.forward);
            moveForwardDist = Mathf.Min(moveForwardDist, forwardDisp - radius - nearClipPlaneDist);
            float upDisp = Mathf.Abs(Vector3.Dot(vec, transform.up)) + radius;
            if (upDisp > nearClipPlaneUpDist / nearClipPlaneDist * (forwardDisp - moveForwardDist)) {
                moveForwardDist = forwardDisp - upDisp * nearClipPlaneDist / nearClipPlaneUpDist;
            }
            float rightDisp = Mathf.Abs(Vector3.Dot(vec, transform.right)) + radius;
            if (rightDisp > nearClipPlaneRightDist / nearClipPlaneDist * (forwardDisp - moveForwardDist)) {
                moveForwardDist = forwardDisp - rightDisp * nearClipPlaneDist / nearClipPlaneRightDist;
            }
        }
        transform.Translate(Vector3.forward * moveForwardDist);
    }
}
