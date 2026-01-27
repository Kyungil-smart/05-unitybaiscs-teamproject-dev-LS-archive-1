using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CircularPlatform : MonoBehaviour
{
    [SerializeField][Range(0, 100)] private float _rotateSpeed;
    [SerializeField] private GameObject _pivot;

    private void Update()
    {
        Rotate();
    }

    private void Rotate()
    {
        transform.RotateAround(_pivot.transform.position, Vector3.up, _rotateSpeed * Time.deltaTime);
    }
}
