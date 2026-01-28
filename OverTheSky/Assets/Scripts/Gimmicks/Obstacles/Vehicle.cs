using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Vehicle : MonoBehaviour
{
    private float _moveSpeed = 50f;
    private float _pushForce = 100f;
    private float _upForce = 10f;
    
    private void FixedUpdate()
    {
        transform.Translate(Vector3.forward * _moveSpeed * Time.deltaTime);
        Destroy(gameObject, 1f);
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            Rigidbody player = other.gameObject.GetComponent<Rigidbody>();

            if (player != null)
            {
                player.freezeRotation = true;
                
                player.AddForce(transform.forward * _pushForce, ForceMode.VelocityChange);
                player.AddForce(Vector3.up * _upForce, ForceMode.VelocityChange);
            }
        }
    }
}
