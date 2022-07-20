using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    // Start is called before the first frame update

    public Transform cameraTransform;
    public Transform minZoomTransform;
    public Transform maxZoomTransform;

    Vector3 lastHit;
    bool isMoving;

    Vector2 lastMousePos;

    public float zoomLevel = 2;
    public int totalZoomLevels = 5;

    public float zoomSpeed = 0.1f;

    float interpolateZoomLevel;

    public float angularSpeed = 10f;

    void Start()
    {
        zoomLevel = Mathf.Min(totalZoomLevels,
                        Mathf.Max(0f,
                            Mathf.Floor(zoomLevel)));
        interpolateZoomLevel = zoomLevel;
        lastMousePos = Vector2.zero;

        isMoving = false;
        lastHit = Vector3.zero;

        UpdateZoomPosition();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButton(0))
        {
            if (Input.GetKey(KeyCode.LeftControl))
                UpdateRotation();
            else
                UpdatePosition();
        }
        else
        {
            isMoving = false;
            lastMousePos = Vector2.zero;
        }
            

        if (Input.mouseScrollDelta.y != 0)
            UpdateZoom(Input.mouseScrollDelta.y > 0);

        if (zoomLevel!=interpolateZoomLevel)
            UpdateZoomPosition();
    }

    void UpdatePosition()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 1000f))
        {
            Vector3 rel_hit = hit.point - transform.position;
            rel_hit.y = 0;

            if (isMoving)
                transform.position += (lastHit - rel_hit);
            

            lastHit = rel_hit;
            isMoving = true;
        }
    }

    void UpdateZoom(bool up)
    {
        if (up) zoomLevel = zoomLevel < totalZoomLevels ? zoomLevel + 1 : totalZoomLevels;
        else    zoomLevel = zoomLevel > 0               ? zoomLevel - 1 : 0;
    }

    void UpdateZoomPosition()
    {
        if (Mathf.Abs(zoomLevel - interpolateZoomLevel) < 0.001f) interpolateZoomLevel = zoomLevel;
        else interpolateZoomLevel += (zoomLevel - interpolateZoomLevel) * zoomSpeed;

        float lerp = interpolateZoomLevel / totalZoomLevels;
        cameraTransform.position = Vector3.Lerp(minZoomTransform.position, maxZoomTransform.position, lerp);
        cameraTransform.rotation = Quaternion.Slerp(minZoomTransform.rotation, maxZoomTransform.rotation, lerp);
    }

    void UpdateRotation()
    {
        Vector3 mouse = Input.mousePosition;

        if (lastMousePos != Vector2.zero)
        {
            float curr_rotation = angularSpeed * (mouse.x - lastMousePos.x)/10f;

            transform.Rotate(0f, curr_rotation, 0f);
        }

        lastMousePos = mouse;
    }
}
