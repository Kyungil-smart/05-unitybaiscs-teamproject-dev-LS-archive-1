using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OverTheSky.Gimmicks.Platforms
{
    public class CircularPlatform : BasePlatform
    {
        [SerializeField][Range(0, 200)] private float _rotateSpeed;
        [SerializeField] private GameObject _pivot;

        private void FixedUpdate()
        {
            _lastPosition = transform.position;

            Rotate();
            MovePassenger();
        }

        private void Rotate()
        {
            // pivot의 Y축을 기준으로 다른 오브젝트가 공전을 할 수 있도록 함
            transform.RotateAround(_pivot.transform.position, Vector3.up, _rotateSpeed * Time.deltaTime);
        }
    }
}
