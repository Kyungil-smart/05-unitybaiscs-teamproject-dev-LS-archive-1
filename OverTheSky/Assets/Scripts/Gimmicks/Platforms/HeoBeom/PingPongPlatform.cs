using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PingPongPlatform : MonoBehaviour
{
    [SerializeField][Range(0, 30)] private float _moveSpeed;
    [SerializeField]private Transform _targetPointA;
    [SerializeField]private Transform _targetPointB;

    private Transform _currentLocate;

    private void Start()
    {
        // 시작 시 이동할 위치를 정해줌
        _currentLocate = _targetPointA;
    }
    
    private void FixedUpdate()
    {
        Move();
    }

    private void Move()
    {  
        // 널 레퍼런스 익셉션 오류가 떠서 null이 아닐 때만 실행
        if(_currentLocate != null)
        {
            transform.position = Vector3.MoveTowards(transform.position, _currentLocate.position, _moveSpeed * Time.deltaTime);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // PingPongTargetA와 충돌 시 _targetPointB로 이동
        if (other.name == "PingPongTargetA")
        {
            _currentLocate = _targetPointB;
        }
        else if (other.name == "PingPongTargetB")
        {
            _currentLocate = _targetPointA;
        }
    }
}
