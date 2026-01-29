using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OverTheSky.Gimmicks.Platforms
{
    public class WaypointPlatform : MonoBehaviour
    {
        [SerializeField][Range(0, 30)] private float _moveSpeed;
        [SerializeField] private Transform _targetPointA;
        [SerializeField] private Transform _targetPointB;
        [SerializeField] private Transform _targetPointC;
        [SerializeField] private Transform _targetPointD;

        private Transform _currentLocate;
        private Vector3 _lastPosition;
        private Rigidbody _playerRigidbody;


        private void Start()
        {
            // 시작 시 발판이 이동할 위치를 정해줌
            _currentLocate = _targetPointA;
        }

        private void FixedUpdate()
        {
            _lastPosition = transform.position;

            Move();
            MovePassenger();
        }

        private void Move()
        {
            // 널 레퍼런스 익셉션 오류가 떠서 null이 아닐 때만 실행하도록
            if (_currentLocate != null)
            {
                transform.position = Vector3.MoveTowards(transform.position, _currentLocate.position, _moveSpeed * Time.deltaTime);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            // 'PingPongTargetA'와 충돌 시 _targetPointB로 이동
            if (other.name == "WaypointPlatformA")
            {
                _currentLocate = _targetPointB;
            }
            // 'PingPongTargetB'와 충돌 시 _targetPoinC로 이동
            else if (other.name == "WaypointPlatformB")
            {
                _currentLocate = _targetPointC;
            }
            // 'PingPongTargetC'와 충돌 시 _targetPoinD로 이동
            else if (other.name == "WaypointPlatformC")
            {
                _currentLocate = _targetPointD;
            }
            // 'PingPongTargetD'와 충돌 시 _targetPoinA로 이동
            else if (other.name == "WaypointPlatformD")
            {
                _currentLocate = _targetPointA;
            }
        }

        private void MovePassenger()
        {
            // 발판의 이동 거리를 계산
            Vector3 platformMoveDistance = transform.position - _lastPosition;

            // 플레이어가 올라타면 같이 이동
            if (_playerRigidbody != null)
            {
                _playerRigidbody.MovePosition(_playerRigidbody.position + platformMoveDistance);
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.CompareTag("Player"))
            {
                // 플레이어의 Rigidbody 컴포넌트를 참조
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
