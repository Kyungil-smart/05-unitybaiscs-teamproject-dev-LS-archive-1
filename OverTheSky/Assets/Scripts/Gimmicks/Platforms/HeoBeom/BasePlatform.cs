using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OverTheSky.Gimmicks.Platforms
{
    // 중복되는 코드들 상속받으려고 따로 뺐습니다.
    public class BasePlatform : MonoBehaviour
    {
        protected Vector3 _lastPosition;
        protected Rigidbody _playerRigidbody;

        protected void MovePassenger()
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
