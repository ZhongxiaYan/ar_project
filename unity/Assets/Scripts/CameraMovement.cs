using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour {
    float zoomSpeed = 6.0f;
    float translateSpeed = 3.0f;

    void Update() {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        transform.Translate(zoomSpeed * scroll * transform.forward);
        if (Input.GetKey("up")) {
            transform.Translate(translateSpeed * transform.up);
        }
        if (Input.GetKey("down")) {
            transform.Translate(translateSpeed * -transform.up);
        }
        if (Input.GetKey("right")) {
            transform.Translate(translateSpeed * transform.right);
        }
        if (Input.GetKey("left")) {
            transform.Translate(translateSpeed * -transform.right);
        }
	}
}
