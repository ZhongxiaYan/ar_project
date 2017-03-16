using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectSelector : MonoBehaviour {

    static Shader standardColor = null;
    static Shader hoverColor = null;
    static Shader selectColor = null;
    static Shader groupColor = null;
    Material objMaterial = null;

    public static Manager manager = null;
    public Vector3 centroid;

    void Start() {
        if (standardColor == null || hoverColor == null || selectColor == null || groupColor == null) {
            standardColor = Shader.Find("Standard");
            hoverColor = Shader.Find("Solid Yellow");
            selectColor = Shader.Find("Solid Blue");
            groupColor = Shader.Find("Solid Green");
        }
        objMaterial = gameObject.GetComponent<Renderer>().material;
    }

    public void UpdateColor() {
        if (manager.selectedObj == this || (manager.hasSelectedGroup && manager.selectedGroup.Contains(this))) {
            objMaterial.shader = selectColor;
        } else if (manager.selectedGroup != null && manager.selectedGroup.Contains(this)) {
            objMaterial.shader = groupColor;
        } else if (manager.hoverObj == this) {
            objMaterial.shader = hoverColor;
        } else {
            objMaterial.shader = standardColor;
        }
    }

    void OnMouseEnter() {
        manager.Hover(this);
    }

    void OnMouseExit() {
        manager.Unhover(this);
    }

    public static void UpdateGroupColor(System.Collections.IEnumerable group) {
        if (group == null) {
            return;
        }
        foreach (ObjectSelector obj in group) {
            obj.UpdateColor();
        }
    }

}
