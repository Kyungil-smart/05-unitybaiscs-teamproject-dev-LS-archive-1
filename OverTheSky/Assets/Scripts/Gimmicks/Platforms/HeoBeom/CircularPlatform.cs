using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OverTheSky.Gimmicks.Platforms
{
    public class CircularPlatform : MonoBehaviour
    {
        [SerializeField][Range(0, 200)] private float _rotateSpeed;
        [SerializeField] private GameObject _pivot;

        private Vector3 _lastPosition;
        private Rigidbody _playerRigidbody;

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

        private void MovePassenger()
        {
            // 발판의 이동거리
            Vector3 platformMoveDistance = transform.position - _lastPosition;

            // 플레이어가 오브젝트에 올라가면 플랫폼과 같이 이동
            if (_playerRigidbody != null)
            {
                _playerRigidbody.MovePosition(_playerRigidbody.position + platformMoveDistance);
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.CompareTag("Player"))
            {   // 플레이어의 Rigidbody 컴포넌트를 참조
                _playerRigidbody = collision.gameObject.GetComponent<Rigidbody>();
            }
        }

        private void OnCollisionExit(Collision collision)
        {
            if (collision.gameObject.CompareTag("Player"))
            {
                _playerRigidbody = null;
            }
        }
    }
}
