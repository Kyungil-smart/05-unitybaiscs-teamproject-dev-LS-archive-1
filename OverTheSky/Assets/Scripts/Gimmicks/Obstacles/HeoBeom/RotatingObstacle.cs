using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
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

        private void OnCollisionEnter(Collision other)
        {
            // 여기서는 공부할 겸 레이어로 접근해봤습니다.
            if (((1 << other.gameObject.layer) & _layerMask.value)!= 0)
            {
                Rigidbody playerRigidbody = other.gameObject.GetComponent<Rigidbody>();

                if (playerRigidbody != null)
                {   
                    // 코루틴을 사용해 날아가는 모션을 조금 더 부드럽게 연출
                    StartCoroutine(SmoothMotion(playerRigidbody));
                }
            }
        }
        
        private IEnumerator SmoothMotion(Rigidbody playerRigidbody)
        {
            float _duration = 0.1f;
            float _elapsed = 0f;
            
            while(_elapsed < _duration)
            {
                Vector3 hitDirection = (-transform.forward + transform.up * 0.3f).normalized;

                // AddForce로 Player에게 힘을 전달
                // _hitForce 수치를 낮추고 ForceMode를 변경 가능(연출적으로 더 맘에 드는 걸로)
                playerRigidbody.AddForce(hitDirection * _rotateSpeed * _hitForce, ForceMode.Force);

                _elapsed += Time.deltaTime;
                yield return null;
            }
        } 
    }
}
