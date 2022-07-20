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

    int zoomLevel = 5;

    int totalZoomLevels = 15;

    void Start()
    {
        isMoving = false;
        UpdateZoomPosition();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButton(0))
            UpdatePosition();
        else
            isMoving = false;

        if (Input.mouseScrollDelta.y != 0)
            UpdateZoom(Input.mouseScrollDelta.y > 0);
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
            {
                transform.position += (lastHit - rel_hit);
            }

            lastHit = rel_hit;
            isMoving = true;
        }
    }

    void UpdateZoom(bool up)
    {
        if (up) zoomLevel = zoomLevel < totalZoomLevels ? zoomLevel + 1 : totalZoomLevels;
        else    zoomLevel = zoomLevel > 0               ? zoomLevel - 1 : 0;

        UpdateZoomPosition();
    }

    void UpdateZoomPosition()
    {
        float lerp = zoomLevel / ((float)totalZoomLevels);
        cameraTransform.position = Vector3.Lerp(minZoomTransform.position, maxZoomTransform.position, lerp);
        cameraTransform.rotation = Quaternion.Slerp(minZoomTransform.rotation, maxZoomTransform.rotation, lerp);
    }
}
