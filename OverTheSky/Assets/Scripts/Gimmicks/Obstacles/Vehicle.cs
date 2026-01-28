using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Vehicle : MonoBehaviour
{
    private float _moveSpeed = 100f;

    private void Update()
    {
        gameObject.transform.Translate(Vector3.forward * _moveSpeed * Time.deltaTime);
        Destroy(gameObject, 0.3f);
    }
}
