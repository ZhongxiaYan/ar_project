using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseEvent : MonoBehaviour {
    bool hasBeenSelected = false;
    public Manager manager = null;

    void OnMouseEnter() {
        if (!hasBeenSelected) {
            gameObject.GetComponent<Renderer>().material.shader = Shader.Find("Self-Illumin/Outlined Diffuse");
        }
    }

    void OnMouseExit() {
        gameObject.GetComponent<Renderer>().material.shader = Shader.Find("Diffuse");
    }

    void OnMouseUpAsButton() {
        if (!hasBeenSelected) {
           manager.AddToQueue(gameObject);
           hasBeenSelected = true;
           gameObject.GetComponent<Renderer>().material.shader = Shader.Find("Diffuse");
        }
    }
}
