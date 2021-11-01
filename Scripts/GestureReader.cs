using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GestureReader : MonoBehaviour  //对手势进行十次采样，并将影响传递至Abilities
{
    private Vector3 startFingerPos;
    private Vector3 endFingerPos;
    private bool fingerMoving;

    private void Awake()
    {
        fingerMoving = false;
    }

    void Update()
    {
        judgeFinger();
    }

    private void judgeFinger()
    {
        if (Input.GetTouch(0).phase == TouchPhase.Began && !fingerMoving)
        {
            fingerMoving = true;
            startFingerPos = Input.GetTouch(0).position;
        }

        if (Input.GetTouch(0).phase == TouchPhase.Moved)
        {
            
        }
        
        if (Input.GetTouch(0).phase == TouchPhase.Ended && fingerMoving)
        {
            fingerMoving = false;
            endFingerPos = Input.GetTouch(0).position;
        }
    }
}
