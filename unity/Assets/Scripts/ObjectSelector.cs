using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectSelector : MonoBehaviour {
    bool isHover = false;
    bool isAddedToQueue = false;

    static Shader standardColor = null;
    static Shader hoverColor = null;
    static Shader pivotColor = null;
    Material objMaterial = null;

    public Manager manager = null;
    public Vector3 centroid;

    void Start() {
        standardColor = Shader.Find("Standard");
        hoverColor = Shader.Find("Solid Yellow");
        pivotColor = Shader.Find("Solid Blue");
        objMaterial = gameObject.GetComponent<Renderer>().material;
    }

    public void UpdateColor() {
        if (isAddedToQueue) {
            objMaterial.shader = standardColor;
        } else if (manager.selectedChildSelector == this) {
            objMaterial.shader = pivotColor;
        } else if (isHover) {
            objMaterial.shader = hoverColor;
        } else {
            objMaterial.shader = standardColor;
        }
    }

    void OnMouseEnter() {
        isHover = true;
        UpdateColor();
    }

    void OnMouseOver() {
        if (Input.GetMouseButtonDown(2)) {
            manager.SetSelected(this);
            UpdateColor();
        }
    }

    void OnMouseExit() {
        isHover = false;
        UpdateColor();
    }

    void OnMouseUpAsButton() {
        manager.AddToQueue(this);
        isAddedToQueue = true;
        UpdateColor();
    }
}
