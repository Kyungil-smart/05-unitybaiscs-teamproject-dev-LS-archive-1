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
        
        // 상태 프로퍼티 / 프로퍼티로 상태 변경에 따른 이벤트 호출 최적화
        private bool _isGrounded;
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
        
        // 천장 / 벽 체크
        public bool IsCeiling { get; private set; }
        public bool IsWall { get; private set; }
        
        // 경사로 계산을 위한 노멀 벡터
        public Vector3 GroundNormal { get; private set; } = Vector3.up;
        public float SlopeAngle { get; private set; }
        
        // 땅 체크용 변수(인스펙터)
        [Header("Ground Check Settings")]
        [SerializeField] private LayerMask _groundLayer;
        // 바닥 체크: 캡슐 반지름보다 살짝 작게 해야 벽에 비비적댈 때 바닥으로 인식 안 함
        [SerializeField] private float _groundCheckRadius = 0.25f;
        [SerializeField] private float _groundCheckOffset = 0.05f; // 바닥 인식 오차 범위 (Skin Width)
        
        // 천장 체크용 변수(인스펙터)
        [Header("Ceiling Check Settings")]
        [SerializeField] private float _ceilingCheckRadius = 0.25f;
        [SerializeField] private float _ceilingCheckOffset = 0.05f;
        
        // 경사 제한
        [SerializeField] [Range(0, 70)]private float _maxSlopeAngle = 50f;
        
        // 디바운싱 (코요테 타임)
        private float _lastGroundedTime = 0f;
        private const float COYOTE_TIME = 0.1f;  // 0.1초 여유
        
        // Normal 판정 임계값
        private const float GROUND_NORMAL_THRESHOLD = 0.7f;
        private const float CEILING_NORMAL_THRESHOLD = -0.1f;
        
        
        protected virtual void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
            _anim = GetComponent<Animator>();
            _col = GetComponent<CapsuleCollider>();

            SetupRigidbody();
        }
        
        private void SetupRigidbody()
        {
            // 리지드바디 기본 세팅 (R&D 문서 반영)
            _rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
            _rigidbody.collisionDetectionMode = CollisionDetectionMode.Continuous;
            _rigidbody.freezeRotation = true;   // 넘어짐 방지
            _rigidbody.useGravity = false;      // 중력 커스텀 제어
        }


        private void FixedUpdate()
        {
            CheckGround();
            CheckCeiling();
        }

        // SphereCast로 바닥 감지
        private void CheckGround()
        {
            // 시작점 설정 캡슐의 끝이 아닌 중심에서 시작해 Raycast의 시작점이 묻히는 문제 해결)
            Vector3 startPos = transform.position + _col.center; 
            
            // 쏘는 거리 계산
            // (캡슐 중앙 높이) - (구 반지름) + (원하는 감지 거리)
            // (캡슐 절반 높이) - (구 반지름) + (원하는 감지 거리)
            float castDistance = (_col.height * 0.5f) - _groundCheckRadius + _groundCheckOffset;
            
            // SphereCast로 바닥 감지
            if (Physics.SphereCast(startPos, _groundCheckRadius, Vector3.down, out RaycastHit hit, castDistance, _groundLayer))
            {
                // 경사각 계산
                SlopeAngle = Vector3.Angle(Vector3.up, hit.normal);
                
                // Normal.y 값으로 표면 종류 판정
                if (hit.normal.y > GROUND_NORMAL_THRESHOLD)
                {
                    // 바닥
                    if (SlopeAngle > _maxSlopeAngle)
                    {
                        // 너무 가파른 경사 - 올라갈 수 없음
                        IsGrounded = false;
                        IsWall = false;
                        GroundNormal = Vector3.up;
                    }
                    else
                    {
                        // 정상적인 바닥
                        IsGrounded = true;
                        IsWall = false;
                        GroundNormal = hit.normal;
                        _lastGroundedTime = Time.time;
                    }
                }
                else if (hit.normal.y < CEILING_NORMAL_THRESHOLD)
                {
                    // 천장 (SphereCast가 위쪽 충돌체를 감지한 경우)
                    IsGrounded = false;
                    IsWall = false;
                    GroundNormal = Vector3.up;
                    SlopeAngle = 0f;
                }
                else
                {
                    // 벽 (거의 수직인 표면)
                    IsGrounded = false;
                    IsWall = true;
                    GroundNormal = hit.normal;
                }
            }
            else
            {
                // 감지 실패
                GroundNormal = Vector3.up;
                SlopeAngle = 0f;
                IsWall = false;
                
                // 코요테 타임 적용
                if (Time.time - _lastGroundedTime <= COYOTE_TIME)
                {
                    IsGrounded = true;
                }
                else
                {
                    IsGrounded = false;
                }
            }
        }
        
        protected virtual void OnLand()
        {
            Logger.Instance.LogInfo($"OnLand! Slope: {SlopeAngle:F1}°");
        }

        protected virtual void OnFall()
        {
            Logger.Instance.LogInfo("OnFall!");
        }
        
        private void CheckCeiling()
        {
            // 천장 체크 위치 계산
            Vector3 topPos = transform.position + Vector3.up * (_col.center.y + (_col.height * 0.5f) - _ceilingCheckRadius);
            Vector3 checkPos = topPos + Vector3.up * _ceilingCheckOffset;

            // CheckSphere로 빠른 판정
            if (Physics.CheckSphere(checkPos, _ceilingCheckRadius, _groundLayer))
            {
                // Raycast로 2차 검증 (Normal 확인)
                // 시작점을 '캡슐의 중심'으로 변경 + 거리는 '절반 높이 + 알파'
                Vector3 startPos = transform.position + _col.center;
                float checkDist = (_col.height * 0.5f) + _ceilingCheckOffset + _ceilingCheckRadius;
                
                // Raycast로 Normal 확인 (천장인지 체크)
                if (Physics.Raycast(
                        startPos,           // 중심에서
                        Vector3.up,       // 위로
                        out RaycastHit hit, 
                        checkDist,              // 머리 끝까지
                        _groundLayer))
                {
                    // Normal이 아래를 향하면 천장
                    IsCeiling = hit.normal.y < CEILING_NORMAL_THRESHOLD;
                }
                else
                {
                    // CheckSphere는 감지했는데 Raycast 실패 - 일단 천장으로 간주
                    IsCeiling = true;
                }
            }
            else
            {
                IsCeiling = false;
            }
        }
        
        // 디버깅용 기즈모 (Scene 뷰에서 바닥 체크 범위 확인)
        private void OnDrawGizmosSelected()
        {
            if (_col == null) _col = GetComponent<CapsuleCollider>();
            if (_col == null) return;
            
            // Ground SphereCast 시각화
            Vector3 startPos = transform.position + _col.center;
            float castDistance = (_col.height * 0.5f) - _groundCheckRadius + _groundCheckOffset;
            
            // 바닥 상태에 따른 색상
            if (IsGrounded)
                Gizmos.color = Color.green;
            else if (IsWall)
                Gizmos.color = Color.magenta;
            else
                Gizmos.color = Color.red;
            
            // 시작 구 (캡슐 내부)
            Gizmos.DrawWireSphere(startPos, _groundCheckRadius);
            // 끝 구 (바닥 닿는 곳)
            Gizmos.DrawWireSphere(startPos + Vector3.down * castDistance, _groundCheckRadius);
            // 연결선
            Gizmos.DrawLine(startPos, startPos + Vector3.down * castDistance);
            
            // Ceiling 체크 영역 시각화
            Gizmos.color = IsCeiling ? Color.yellow : Color.cyan;
            Vector3 topPos = transform.position + Vector3.up * (_col.center.y + (_col.height * 0.5f) - _ceilingCheckRadius);
            Vector3 checkPos = topPos + Vector3.up * _ceilingCheckOffset;
            Gizmos.DrawWireSphere(checkPos, _ceilingCheckRadius);
            
            // Ground Normal 시각화
            if (IsGrounded)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawRay(transform.position, GroundNormal * 0.5f);
                
                // 경사각 표시 (Scene 뷰에서만 보임)
#if UNITY_EDITOR
                UnityEditor.Handles.Label(
                    transform.position + Vector3.up * 2f, 
                    $"Slope: {SlopeAngle:F1}°"
                );
#endif
            }
        }
    }
}