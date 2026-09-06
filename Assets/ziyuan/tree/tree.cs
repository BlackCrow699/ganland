using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class tree : MonoBehaviour
{
    void LateUpdate()
    {
        Vector3 camPos = Camera.main.transform.position;
        Vector3 targetPos = new Vector3(camPos.x, transform.position.y, camPos.z);
        transform.LookAt(targetPos);
        transform.Rotate(-90f, -90f, 0f, Space.Self);
        
        
    }
}
