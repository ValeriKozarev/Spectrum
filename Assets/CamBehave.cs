using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamBehave : MonoBehaviour
{

    public Transform lookAt;
    public Transform camTransform;

    private Camera cam;

    private float distance = 30.0f;
    private float currentX = 0.0f;
    private float currentY = 0.0f;
    private float sensitivityX = 4.0f;
    private float sensitivityY = 1.0f;

    private void Start ()
    {
        camTransform = lookAt;
        cam = Camera.main;
    }

    private void LateUpdate()
    {
        Vector3 dir = new Vector3(10,0,-distance);
        
    }
}