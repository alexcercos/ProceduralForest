using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    // Start is called before the first frame update

    Vector3 last_hit;
    bool is_moving;

    void Start()
    {
        is_moving = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButton(0))
        {

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 1000f))
            {
                Vector3 rel_hit = hit.point - transform.position;
                rel_hit.y = 0;
                if (is_moving)
                {
                    transform.position += (last_hit - rel_hit);
                }

                last_hit = rel_hit;
                is_moving = true;
            }
                
        }
        else
        {
            is_moving = false;
        }
    }
}
