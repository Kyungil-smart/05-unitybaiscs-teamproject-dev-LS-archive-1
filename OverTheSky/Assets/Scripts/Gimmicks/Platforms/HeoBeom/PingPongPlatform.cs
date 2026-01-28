using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PingPongPlatform : MonoBehaviour
{
    [SerializeField][Range(0, 100)] private float _moveSpeed;
    [SerializeField]private GameObject _targetPointA;
    [SerializeField]private GameObject _targetPointB;

    private void Update()
    {
        Move();
    }

    private void Move()
    {
        if (gameObject.transform.position == _targetPointB.transform.position)
        {
            transform.position = Vector3.MoveTowards(transform.position, _targetPointA.transform.position , Time.deltaTime * _moveSpeed);
        }
        if (gameObject.transform.position == _targetPointA.transform.position)
        {
            transform.position = Vector3.MoveTowards(transform.position, _targetPointB.transform.position, Time.deltaTime * _moveSpeed);
        }
    }
}
