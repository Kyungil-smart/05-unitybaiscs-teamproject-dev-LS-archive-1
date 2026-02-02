using UnityEngine;

namespace OverTheSky.Core
{
    [RequireComponent(typeof(Rigidbody))]
    public class ForceReceiver : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private float _drag = 5f; // 감쇠 속도 (기존 impactDamping)
        [SerializeField] private float _threshold = 0.2f; // 무시할 최소 충격량

        private Vector3 _impactVelocity;
        private Rigidbody _rigidbody;

        public Vector3 Movement => _impactVelocity; // 외부에서 가져갈 현재 충격 속도

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
        }

        private void FixedUpdate()
        {
            // 충격량 감쇠 (Damping)
            if (_impactVelocity.sqrMagnitude > _threshold * _threshold)
            {
                _impactVelocity = Vector3.Lerp(_impactVelocity, Vector3.zero, _drag * Time.fixedDeltaTime);
            }
            else
            {
                // 충격량이 최소 충격량보다 적다면 _impactVelocity를 0으로 설정
                _impactVelocity = Vector3.zero;
            }
        }
        
        // 외부에서 AddForce 대신 호출할 함수(기본적으로는 Impulse)
        public void AddImpact(Vector3 force, ForceMode mode = ForceMode.Impulse)
        {
            float mass = _rigidbody.mass;
            
            Logger.Instance.LogInfo($"Adding impact for {force}, ForceMode: {mode}");
            switch (mode)
            {
                case ForceMode.Force:
                    // 힘(N)을 지속적으로 가함: v += (F / m) * t
                    // 주로 매 프레임 호출되는 로직(바람, 지속적인 밀기)에 적합
                    _impactVelocity += force / mass * Time.fixedDeltaTime;
                    break;
                case ForceMode.Acceleration:
                    // 가속도를 지속적으로 가함 (질량 무시): v += a * t
                    // 매 프레임 호출되는 로직(중력장, 컨베이어 벨트)에 적합
                    _impactVelocity += force * Time.fixedDeltaTime;
                    break;
                case ForceMode.Impulse:
                    // 순간적인 충격량(N*s)을 가함: v += F / m
                    // 폭발, 타격 등 단발성 충격에 적합
                    _impactVelocity += force / mass;
                    break;
                case ForceMode.VelocityChange:
                    // 순간적인 속도 변경 (질량 무시): v += v_delta
                    // 질량에 상관없이 똑같은 속도로 날리고 싶을 때 사용 (대시, 넉백 고정값)
                    _impactVelocity += force;
                    break;
            }
        }

        // 충격 초기화 (필요시)
        public void Reset()
        {
            _impactVelocity = Vector3.zero;
        }
    }
}