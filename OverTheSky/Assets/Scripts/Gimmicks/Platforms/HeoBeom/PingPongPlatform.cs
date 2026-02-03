using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace OverTheSky.Gimmicks.Platforms
{
    public class PingPongPlatform : BasePlatform
    {
        [SerializeField][Range(0, 30)] private float _moveSpeed;
        [SerializeField]private Transform _targetPointA;
        [SerializeField]private Transform _targetPointB;

        private Transform _currentLocate;

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
            if(_currentLocate != null)
            {
                transform.position = Vector3.MoveTowards(transform.position, _currentLocate.position, _moveSpeed * Time.deltaTime);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            // 'PingPongTargetA'와 충돌 시 _targetPointB로 이동
            if (other.name == "PingPongTargetA")
            {
                _currentLocate = _targetPointB;
            }
            // 'PingPongTargetB'와 충돌 시 _targetPoinA로 이동
            else if (other.name == "PingPongTargetB")
            {
                _currentLocate = _targetPointA;
            }
        }
    }
}
