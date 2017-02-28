using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class User_Camera_Movement : MonoBehaviour
{
    public float speed = 0.1F;
    float zoomSpeed = 200f;
    float translateSpeed = 100f;
    float turnSpeed = 100f;

    void Update()
    {
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Moved)
        {
            Debug.Log("Touch");
            // Get movement of the finger since last frame
            Vector2 touchDeltaPosition = Input.GetTouch(0).deltaPosition;

            // Move object across XY plane
            this.transform.Translate(-touchDeltaPosition.x * speed, -touchDeltaPosition.y * speed, 0);
        }
    }
}
