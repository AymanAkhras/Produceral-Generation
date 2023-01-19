using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraPanOrtho : MonoBehaviour
{
    public float responsiveness = 1.0f;
    public float sensitivity = 75.0f;

    public float camDistanceMax = 200f;
    public float camDistanceMin = -100f;
    public float scrollSpeed = 50f;

    float cameraDistance = 0f;

    Vector3 offsetPos;
    float offsetZoom;
    Vector3 panTarget;

    void Start()
    {
        offsetPos = transform.position;
        offsetZoom = GetComponent<Camera>().orthographicSize;
    }


    void Update()
    {

        float x = Input.mousePosition.x;
        float y = Input.mousePosition.y;

        if (x < 0)
        {
            panTarget -= new Vector3(sensitivity * Time.deltaTime, 0, 0);
        }
        else if (x > Screen.width)
        {
            panTarget += new Vector3(sensitivity * Time.deltaTime, 0, 0);
        }
        if (y < 0)
        {
            panTarget -= new Vector3(0, 0, sensitivity * Time.deltaTime);
        }
        else if (y > Screen.height)
        {
            panTarget += new Vector3(0, 0, sensitivity * Time.deltaTime);
        }

        // scroll zoom
        cameraDistance += Input.GetAxis("Mouse ScrollWheel") * -scrollSpeed;
        cameraDistance = Mathf.Clamp(cameraDistance, camDistanceMin, camDistanceMax);

        
        GetComponent<Camera>().orthographicSize = offsetZoom + cameraDistance;

        transform.position = Vector3.Lerp(transform.position, offsetPos + panTarget, responsiveness * Time.deltaTime);
    }

}