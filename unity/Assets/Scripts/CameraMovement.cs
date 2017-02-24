using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour {
    float zoomSpeed = 200f;
    float translateSpeed = 100f;
    float turnSpeed = 100f;

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
}
