using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using OverTheSky.Core;
using OverTheSky.Player;
using Unity.VisualScripting;
using UnityEngine;

namespace OverTheSky.Gimmicks.Obstacles
{
    public class RotatingObstacle : MonoBehaviour
    {
        [SerializeField][Range(0, 500)] private float _rotateSpeed;
        [SerializeField][Range(0, 10)] private float _hitForce;
        [SerializeField] private LayerMask _layerMask;

        private void FixedUpdate()
        {
            Rotation();
        }

        private void Rotation()
        {
            // FixedUpdate에서 사용시 Time.deltaTime이 아닌 Time.fixedDeltaTime을 사용해야함
            transform.Rotate(Vector3.up * (_rotateSpeed * Time.fixedDeltaTime));
        }

        private void OnCollisionEnter(Collision other)
        {
            // 여기서는 공부할 겸 레이어로 접근해봤습니다.
            if (((1 << other.gameObject.layer) & _layerMask.value)!= 0)
            {
                // 힘을 가할 리시버를 가져옴
                ForceReceiver receiver = other.gameObject.GetComponent<ForceReceiver>();
                if (receiver != null)
                {
                    // 충돌 방향 계산
                    Vector3 dir = (other.transform.position - transform.position).normalized;
                    dir.y = 0.2f; // 살짝 위로 띄움

                    // 충격량 계산 (회전속도 * 힘 계수)
                    // 계산된 힘에 0.1f 같은 작은 수를 곱해 게임상 적합한 연출의 힘으로 조정
                    float power = (_rotateSpeed * _hitForce) * 0.1f; 

                    // [Impulse 사용] 질량을 고려하여 순간적으로 밀어냄
                    // receiver.AddImpact(dir * power, ForceMode.Impulse);
                    
                    // 부드럽게 밀기 (SmoothMotion)
                    // 코루틴이 중복 실행되지 않게 하려면 StopCoroutine을 쓰거나 플래그를 둘 수 있지만,
                    // 장애물 특성상 그냥 실행해도 무방합니다.
                    StartCoroutine(SmoothMotion(receiver, dir, power));
                }
                
                // 플레이어 호환을 위해 주석처리
                /*Rigidbody playerRigidbody = other.gameObject.GetComponent<Rigidbody>();

                if (playerRigidbody != null)
                {   
                    // 코루틴을 사용해 날아가는 모션을 조금 더 부드럽게 연출
                    StartCoroutine(SmoothMotion(playerRigidbody));
                }*/
            }
        }
         
        // 매개변수를 추가하여 외부에서 계산한 방향과 힘을 그대로 가져온다.
        private IEnumerator SmoothMotion(ForceReceiver receiver, Vector3 direction, float totalPower)
        {
            float _duration = 0.1f; // 미는 시간
            float _elapsed = 0f;
            
            // ForceMode.Force는 "지속적인 힘"이므로, 
            // 짧은 시간(duration) 동안 힘을 나누어 가하는 것이 아니라
            // 해당 시간 동안 "계속해서 강한 힘"을 주는 방식
            
            // Impulse와 비슷한 총량의 힘을 내려면 Force는 더 큰 값이 필요하다.
            float pushForce = totalPower * 5f;
            
            while(_elapsed < _duration)
            {
                // ForceReceiver가 있다면 안전하게 호출
                if (receiver != null)
                {
                    // ForceMode.Force: 질량 적용, 지속 힘
                    // 매 프레임 힘을 가해서 "밀리는" 연출
                    receiver.AddImpact(direction * pushForce, ForceMode.Force);
                }
                
                //Vector3 hitDirection = (-transform.forward + transform.up * 0.3f).normalized;

                // AddForce로 Player에게 힘을 전달
                // _hitForce 수치를 낮추고 ForceMode를 변경 가능(연출적으로 더 맘에 드는 걸로)
                // playerRigidbody.AddForce(hitDirection * _rotateSpeed * _hitForce, ForceMode.Force);
                
                _elapsed += Time.deltaTime;
                yield return null;
            }
        }
        // 플레이어가 아닌 오브젝트를 날리고 싶다면 똑같은 방식으로 코루틴을 짜서 AddForce 방식으로 날리면 가능
    }
}
