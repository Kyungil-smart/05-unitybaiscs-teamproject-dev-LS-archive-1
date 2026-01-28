using UnityEngine;
using OverTheSky.Core;
using Logger = OverTheSky.Core.Logger;

namespace OverTheSky.Player
{
    public class PlayerBase : MonoBehaviour
    {
        // 자식이 갖다 쓸 컴포넌트 : 상속 받을 자식만 사용 허가(protected)
        protected Rigidbody _rigidbody;
        protected Animator _anim;
        protected CapsuleCollider _col;
        
        // 디바운싱용
        private float _lastGroundedTime = 0f;
        private float _groundedDebounceTime = 0.1f;  // 0.1초 여유
        
        private bool _isGrounded;
        // 외부에서 땅체크 상태 확인 / 프로퍼티로 상태 변경에 따른 이벤트 호출 최적화
        public bool IsGrounded 
        { 
            get => _isGrounded;
            private set
            {
                if (_isGrounded != value)  // 값이 바뀔 때만
                {
                    bool wasGrounded = _isGrounded;
                    _isGrounded = value;
                
                    // 땅에 닿는 순간 / 떨어지는 순간 이벤트 처리용 가상함수 호출
                    if (!wasGrounded && IsGrounded) OnLand();
                    if (wasGrounded && !IsGrounded) OnFall();
                }
            }
        }
        
        // 땅 체크용 변수(인스펙터)
        [Header("Ground Check Settings")]
        [SerializeField] private LayerMask _groundLayer;
        // 값이 작을수록 바닥 판정이 민감함
        [SerializeField] private float _groundCheckOffset = 0.15f;
        // Sphere 캐스트 Radius
        [SerializeField] private float _groundCheckRadius = 0.25f;
        
        // 경사로 계산을 위한 노멀 벡터
        public Vector3 GroundNormal { get; private set; } = Vector3.up;
        
        protected virtual void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
            _anim = GetComponent<Animator>();
            _col = GetComponent<CapsuleCollider>();

            // 리지드바디 기본 세팅 (R&D 문서 반영)
            _rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
            _rigidbody.collisionDetectionMode = CollisionDetectionMode.Continuous;
            _rigidbody.freezeRotation = true; // 넘어짐 방지
            _rigidbody.useGravity = false;    // 중력 커스텀 제어
        }

        private void FixedUpdate()
        {
            CheckGround();
        }

        private void CheckGround()
        {
            // 캡슐 바닥 위치
            float capsuleBottom = _col.center.y - (_col.height / 2f);
            // 시작점: 캡슐 바닥 + 구 반지름 위치
            Vector3 spherePos = transform.position + Vector3.up * (capsuleBottom + _groundCheckRadius);
            // 체크 거리 (반지름 + 체크 오프셋)
            float castDistance = _groundCheckRadius + _groundCheckOffset;
            
            bool wasGrounded = IsGrounded;
            bool detected = false;
            
            // SphereCast로 바닥 감지
            if (Physics.SphereCast(
                    spherePos, 
                    _groundCheckRadius, 
                    Vector3.down, 
                    out RaycastHit hit, 
                    castDistance, 
                    _groundLayer))
            {
                detected = true;
                GroundNormal = hit.normal;
                _lastGroundedTime = Time.time;
            }
            else
            {
                GroundNormal = Vector3.up;
            }

            if (detected)
            {
                IsGrounded = true;
            }
            else if (Time.time - _lastGroundedTime < _groundedDebounceTime)
            {
                IsGrounded = true; // _groundedDebounceTime(0.1초)보다 게임이 시작하고 지난 시간에서 지난번 땅에 닿은 시간이 작을 경우 아직 땅에 있다는 처리.
            }
            else
            {
                IsGrounded = false;
            }
        }

        protected virtual void OnLand()
        {
            Logger.Instance.LogInfo("OnLand!");
        }

        protected virtual void OnFall()
        {
            Logger.Instance.LogInfo("OnFall!");
        }
        
        // 디버깅용 기즈모 (Scene 뷰에서 바닥 체크 범위 확인)
        private void OnDrawGizmosSelected()
        {
            if (_col == null) return;
    
            float capsuleBottom = _col.center.y - (_col.height / 2f);
            Vector3 spherePos = transform.position + Vector3.up * (capsuleBottom + _groundCheckRadius);
    
            // 시작점
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(spherePos, _groundCheckRadius);
    
            // 끝점
            float castDistance = _groundCheckRadius + _groundCheckOffset;
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(spherePos + Vector3.down * castDistance, _groundCheckRadius);
    
            // 연결선
            Gizmos.color = Color.red;
            Gizmos.DrawLine(spherePos, spherePos + Vector3.down * castDistance);
        }
    }
}
