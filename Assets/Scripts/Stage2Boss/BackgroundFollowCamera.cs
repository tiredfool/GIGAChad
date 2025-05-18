using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine;

public class BackgroundFollowCamera : MonoBehaviour
{
    public Transform cameraTransform;
    private Vector3 offset;

    void Start()
    {
        offset = transform.position - cameraTransform.position;
    }

    void LateUpdate()
    {
        transform.position = cameraTransform.position + offset;
    }
}

