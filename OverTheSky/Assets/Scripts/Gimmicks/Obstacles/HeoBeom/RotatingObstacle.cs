using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OverTheSky.Gimmicks.Obstacles
{
    public class RotatingObstacle : MonoBehaviour
    {
        [SerializeField][Range(0, 200)] private float _rotateSpeed;
        [SerializeField][Range(0, 10)] private float _hitForce;
        [SerializeField] private LayerMask _layerMask;

        private void FixedUpdate()
        {
            Rotation();
        }

        private void Rotation()
        {
            transform.Rotate(Vector3.up * _rotateSpeed * Time.deltaTime);
        }

        private void OnCollisionEnter(Collision collision)
        {
            Debug.Log("Ãæµ¹");
        }
    }
}
