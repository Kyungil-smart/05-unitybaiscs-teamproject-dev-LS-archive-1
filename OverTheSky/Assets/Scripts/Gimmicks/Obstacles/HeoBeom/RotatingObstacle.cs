using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OverTheSky.Gimmicks.Obstacles
{
    public class RotatingObstacle : MonoBehaviour
    {
        [SerializeField][Range(0, 200)] private float _rotateSpeed;
        [SerializeField][Range(0, 1)] private float _hitForce;
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
                    // right이 날아가는 방향이 가장 자연스러우나 완벽하지 않음 하지만 더 좋은 방법이 생각 안 남
                    Vector3 hitDirection = transform.right;

                    // AddForce로 Player에게 힘을 전달
                    playerRigidbody.AddForce(hitDirection * _rotateSpeed * _hitForce, ForceMode.Impulse);
                }
            }
        }
    }
}
