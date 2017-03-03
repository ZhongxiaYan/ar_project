using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectSelector : MonoBehaviour {
    bool isAddedToQueue = false;

    static Shader standardColor = null;
    static Shader hoverColor = null;
    static Shader pivotColor = null;
    static Shader renameColor = null;
    Material objMaterial = null;

    public static Manager manager = null;
    public Vector3 centroid;

    void Start() {
        if (standardColor == null && hoverColor == null && pivotColor == null) {
            standardColor = Shader.Find("Standard");
            hoverColor = Shader.Find("Solid Yellow");
            pivotColor = Shader.Find("Solid Blue");
            renameColor = Shader.Find("Solid Green");
        }
        objMaterial = gameObject.GetComponent<Renderer>().material;
    }

    public void UpdateColor() {
        if (manager.pivotChildSelector == this) {
            objMaterial.shader = pivotColor;
        } else if (manager.renameChildSelectors.Contains(this)) {
            objMaterial.shader = renameColor;
        } else if (manager.hoverChildSelector == this) {
            objMaterial.shader = hoverColor;
        } else {
            objMaterial.shader = standardColor;
        }
    }

    void OnMouseEnter() {
        manager.hoverChildSelector = this;
        UpdateColor();
    }

    void OnMouseExit() {
        if (manager.hoverChildSelector == this) {
            manager.hoverChildSelector = null;
        }
        UpdateColor();
    }

    void OnMouseUpAsButton() {
        if (isAddedToQueue) {
            return;
        }
        manager.LeftClick(this);
        isAddedToQueue = true;
        UpdateColor();
    }
}
