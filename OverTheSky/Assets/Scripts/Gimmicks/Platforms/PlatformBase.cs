using UnityEngine;

namespace OverTheSky.Gimmicks.Platforms
{
    [RequireComponent(typeof(Rigidbody))]
    public abstract class PlatformBase : MonoBehaviour
    {
        protected Rigidbody rb;
        
        // 플레이어가 발판 위에 탔을 때, 이 값을 읽어가서 미끄러짐을 방지함
        public Vector3 CurrentVelocity { get; private set; }
        
        protected virtual void Awake()
        {
            rb = GetComponent<Rigidbody>();
            
            // 리지드바디 세팅 권장값 자동 적용
            rb.isKinematic = true; 
            rb.useGravity = false;
            rb.interpolation = RigidbodyInterpolation.Interpolate; // 부드러운 움직임 필수
        }
        
        // [핵심] 물리 이동 로직은 부모가 통합 관리
        protected virtual void FixedUpdate()
        {
            // 자식 클래스에게 다음 프레임에 어디 있을지 체크
            Vector3 nextPosition = CalculateNextPosition();
            
            // 속도 계산 (이동 거리 / 시간)
            // Time.fixedDeltaTime이 0일 경우(일시정지 등) 0으로 예외처리
            if (Time.fixedDeltaTime > 0)
            {
                CurrentVelocity = (nextPosition - rb.position) / Time.fixedDeltaTime;
            }
            else
            {
                CurrentVelocity = Vector3.zero;
            }
            
            // 3. 실제 물리 이동 적용 (MovePosition을 써야 플레이어가 안 뚫고 지나감)
            rb.MovePosition(nextPosition);
        }
        
        // [구현 필수] 자식 클래스는 이 메서드만 오버라이드해서 로직을 짜면 됨
        // 예: 왕복이면 A <-> B 사이의 Lerp 값, 회전이면 Sin/Cos 값 등
        protected abstract Vector3 CalculateNextPosition();
    }
}