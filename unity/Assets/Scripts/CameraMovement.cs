using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour {
    float zoomSpeed = 200f;
    float translateSpeed = 100f;
    float turnSpeed = 100f;


    Manager manager = null;
    ObjectSelector lastTargetSelector = null;
    Vector3 target;

    void Start() {
        GameObject managerObject = GameObject.Find("Manager");
        manager = managerObject.GetComponent<Manager>();
    }

	void LateUpdate() {
        if (manager.isTextFieldSelected) {
            return;
        }
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        transform.Translate(zoomSpeed * Time.deltaTime * scroll * Vector3.forward);
        if (Input.GetKey(KeyCode.LeftControl)) {
            if (Input.GetKey(KeyCode.UpArrow)) {
                transform.Translate(translateSpeed * Time.deltaTime * Vector3.up);
            }
            if (Input.GetKey(KeyCode.DownArrow)) {
                transform.Translate(translateSpeed * Time.deltaTime * -Vector3.up);
            }
            if (Input.GetKey(KeyCode.RightArrow)) {
                transform.Translate(translateSpeed * Time.deltaTime * Vector3.right);
            }
            if (Input.GetKey(KeyCode.LeftArrow)) {
                transform.Translate(translateSpeed * Time.deltaTime * -Vector3.right);
            }
        } else {
            if (manager.selectedChildSelector == null) {
                if (Input.GetKey(KeyCode.UpArrow)) {
                    transform.Rotate(Vector3.right, turnSpeed * Time.deltaTime);
                }
                if (Input.GetKey(KeyCode.DownArrow)) {
                    transform.Rotate(Vector3.right, -turnSpeed * Time.deltaTime);
                }
                if (Input.GetKey(KeyCode.Period)) {
                    transform.Rotate(Vector3.up, turnSpeed * Time.deltaTime);
                }
                if (Input.GetKey(KeyCode.Comma)) {
                    transform.Rotate(Vector3.up, -turnSpeed * Time.deltaTime);
                }
                lastTargetSelector = null;
            } else {
                if (lastTargetSelector != manager.selectedChildSelector) {
                    lastTargetSelector = manager.selectedChildSelector;
                    target = lastTargetSelector.gameObject.transform.TransformPoint(manager.selectedChildSelector.centroid);
                    transform.LookAt(target);
                }
                if (Input.GetKey(KeyCode.UpArrow)) {
                    transform.RotateAround(target, Vector3.right, turnSpeed * Time.deltaTime);
                }
                if (Input.GetKey(KeyCode.DownArrow)) {
                    transform.RotateAround(target, Vector3.right, -turnSpeed * Time.deltaTime);
                }
                if (Input.GetKey(KeyCode.Period)) {
                    transform.RotateAround(target, Vector3.up, turnSpeed * Time.deltaTime);
                }
                if (Input.GetKey(KeyCode.Comma)) {
                    transform.RotateAround(target, Vector3.up, -turnSpeed * Time.deltaTime);
                }
            }
            if (Input.GetKey(KeyCode.RightArrow)) {
                transform.Rotate(Vector3.forward, turnSpeed * Time.deltaTime);
            }
            if (Input.GetKey(KeyCode.LeftArrow)) {
                transform.Rotate(Vector3.forward, -turnSpeed * Time.deltaTime);
            }
        }
	}
}
