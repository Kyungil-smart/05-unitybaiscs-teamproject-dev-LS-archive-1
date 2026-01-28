using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleThirdPersonCamera : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private Vector3 offset = new Vector3(0f, 2f, -5f);
    [SerializeField] private float followSmooth = 10f;

    private void LateUpdate()
    {
        if (target == null) return;

        Vector3 desiredPos = target.position + target.TransformDirection(offset);
        transform.position = Vector3.Lerp(transform.position, desiredPos, followSmooth * Time.deltaTime);

        transform.LookAt(target.position + Vector3.up * 1.5f);
    }
}

