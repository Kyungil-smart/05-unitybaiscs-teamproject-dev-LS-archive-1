using OverTheSky.Core;
using UnityEngine;

namespace OverTheSky.Player
{
    public class PlayerController : PlayerBase
    {
        [Header("Movement Settings")] 
        [Tooltip("걷기 속도 - Blend Tree Threshold 2에 맞춤")]
        [SerializeField] private float _moveSpeed = 2f;
        [Tooltip("달리기 속도 - Blend Tree Threshold 6에 맞춤")]
        [SerializeField] [Range(0, 20)] private float _sprintSpeed = 6f;
        [Tooltip("가속/감속 속도 - 높을수록 반응이 빠름")]
        [SerializeField] private float _acceleration = 15f;
        [Tooltip("회전 속도 - 높을수록 빠르게 회전")]
        [SerializeField] private float _rotationSpeed = 10f;

        [Header("Jump Settings")]
        [Tooltip("점프 높이 (m)")]
        [SerializeField] private float _jumpHeight = 1.2f;
        [Tooltip("중력 가속도 (음수값)")]
        [SerializeField] private float _gravity = -15f;
        [Tooltip("점프 후 다시 점프 가능할 때까지 시간 (초)")]
        [SerializeField] private float _jumpTimeout = 0.15f;
        
        [Header("Drag Settings")]
        [Tooltip("땅에 있을 때 공기 저항 (높을수록 빨리 감속)")]
        [SerializeField] private float _groundDrag = 5f;
        [Tooltip("공중에 있을 때 공기 저항 (낮을수록 공중 제어 쉬움)")]
        [SerializeField] private float _airDrag = 1f;
        
        
        // 물리 상태
        private float _verticalVelocity;              // 현재 수직 속도 (Y축)
        private float _terminalVelocity = 53f;        // 최대 낙하 속도
        private float _jumpTimeoutDelta;              // 점프 쿨타임 타이머
        
        // 입력 상태 (InputManager에서 받아옴)
        private Vector2 _currentMoveInput;            // 현재 이동 입력 (WASD)
        private bool _isSprinting;                    // 달리기 중인지 체크
        private bool _jumpInput;                      // 점프 입력 체크
        private Camera mainCamera;

        protected override void Awake()
        {
            mainCamera = Camera.main;
            base.Awake();
        }
        
        private void Start()
        {
            // InputManager 이벤트 구독
            if (InputManager.Instance != null)
            {
                InputManager.Instance.OnMove += HandleMoveChange;
                InputManager.Instance.OnSprintKeyDown += HandleSprintChange;
                InputManager.Instance.OnJumpKeyDown += HandleJumpChange;
            }
            
            // 점프 타임아웃 초기화
            _jumpTimeoutDelta = _jumpTimeout;
        }

        private void OnDestroy()
        {
            // 메모리 누수 방지를 위한 이벤트 구독 해제
            if (InputManager.Instance != null)
            {
                InputManager.Instance.OnMove -= HandleMoveChange;
                InputManager.Instance.OnSprintKeyDown -= HandleSprintChange;
                InputManager.Instance.OnJumpKeyDown -= HandleJumpChange;
            }
        }
        
        private void Update()
        {
            // 매 프레임 중력/점프 처리
            JumpAndGravity();
        }
        
        private void FixedUpdate()
        {
            // Drag 조절 (땅/공중 상태에 따라)
            _rigidbody.drag = IsGrounded ? _groundDrag : _airDrag;
            // 이동 처리
            Move();
            // 애니메이션 업데이트
            UpdateAnimation();
        }

        private void Move()
        {
            // 목표 속도 결정 (걷기 or 달리기)
            float targetSpeed = _isSprinting ? _sprintSpeed : _moveSpeed;
            
            // 입력이 없으면 즉시 정지
            if (_currentMoveInput.magnitude < 0.01f)
            {
                if (IsGrounded)
                {
                    // 땅에서: 수평 속도를 0으로 (Y축은 유지)
                    _rigidbody.velocity = new Vector3(0f, _rigidbody.velocity.y, 0f);
                }
                else
                {
                    // 공중에서: 부드럽게 감속 (공중 제어 가능하도록)
                    Vector3 currentHorizontal = new Vector3(_rigidbody.velocity.x, 0f, _rigidbody.velocity.z);
                    Vector3 decelerated = Vector3.MoveTowards(
                        currentHorizontal, 
                        Vector3.zero, 
                        _acceleration * 0.5f * Time.fixedDeltaTime  // 절반 속도로 감속
                    );
                    _rigidbody.velocity = new Vector3(decelerated.x, _rigidbody.velocity.y, decelerated.z);
                }
                return;
            }
            
            // 카메라 방향 기준으로 이동 방향 계산
            Vector3 moveDirection = GetCameraRelativeMovement(_currentMoveInput);
            
            // 이동 방향으로 캐릭터 회전
            if (moveDirection.magnitude > 0.01f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
                transform.rotation = Quaternion.Slerp(
                    transform.rotation, 
                    targetRotation, 
                    _rotationSpeed * Time.fixedDeltaTime
                );
            }
            
            // 경사면 보정 (땅에 있을 때만)
            if (IsGrounded)
            {
                moveDirection = Vector3.ProjectOnPlane(moveDirection, GroundNormal).normalized;
            }
            
            // 목표 속도 벡터 (방향 * 속도)
            Vector3 targetVelocity = moveDirection * targetSpeed;
            
            // 현재 수평 속도 (Y축 제외)
            Vector3 currentHorizontalVelocity = new Vector3(_rigidbody.velocity.x, 0f, _rigidbody.velocity.z);
            
            // 부드럽게 가속 (MoveTowards로 선형 보간)
            Vector3 newHorizontalVelocity = Vector3.MoveTowards(
                currentHorizontalVelocity,
                targetVelocity,
                _acceleration * Time.fixedDeltaTime
            );
            
            // Velocity 직접 제어 (Y축은 유지, XZ만 업데이트)
            _rigidbody.velocity = new Vector3(
                newHorizontalVelocity.x, 
                _rigidbody.velocity.y,  // 중력/점프 영향 유지
                newHorizontalVelocity.z
            );
        }
        
        private Vector3 GetCameraRelativeMovement(Vector2 input)
        {
            Transform cameraTransform = mainCamera.transform;
            Vector3 forward = cameraTransform.forward;
            Vector3 right = cameraTransform.right;
            
            forward.y = 0f;
            right.y = 0f;
            
            forward.Normalize();
            right.Normalize();
            
            return (forward * input.y + right * input.x).normalized;
        }
        
        private void JumpAndGravity()
        {
            if (IsGrounded)
            {
                // 땅에 닿았을 때 수직 속도 초기화
                if (_verticalVelocity < 0f)
                {
                    // 0이 아닌 약간의 음수 값을 주어야 경사면에서 살짝 뜨는 현상을 막고 IsGrounded 판정을 안정적으로 유지
                    _verticalVelocity = -2f;
                }
                
                // 점프 입력 처리
                // _jumpInput은 HandleJumpChange에서 true가 됨
                if (_jumpInput && _jumpTimeoutDelta <= 0f && !IsCeiling)
                {
                    // v = sqrt(H * -2 * G)
                    // 원하는 높이(h)에 도달하기 위한 초기 속도(v)를 계산
                    _verticalVelocity = Mathf.Sqrt(_jumpHeight * -2f * _gravity);

                    _anim.SetBool(Define.Anim.IsJump, true);
                    
                    _jumpTimeoutDelta = _jumpTimeout;
                    
                    // 점프를 수행했으므로 false로 리셋
                    // InputManager가 계속 true를 보내도, 물리적으로 한 번 뛰었으면 꺼야 함
                    _jumpInput = false;
                }
                
                if (_jumpTimeoutDelta >= 0f)
                {
                    _jumpTimeoutDelta -= Time.deltaTime;
                }
            }
            else
            {
                // 현재 로직상 땅에 닿아야만 점프가 되므로 안전하게 끔
                _jumpInput = false;
            }
            
            // 중력 적용
            if (_verticalVelocity > _terminalVelocity)
            {
                _verticalVelocity += _gravity * Time.deltaTime;
            }
            
            _rigidbody.velocity = new Vector3(
                _rigidbody.velocity.x, 
                _verticalVelocity, 
                _rigidbody.velocity.z
            );
        }
        
        private void UpdateAnimation()
        {
            // Speed: 0 (Idle), 0.5 (Walk), 1 (Run)
            float currentSpeed = new Vector3(_rigidbody.velocity.x, 0f, _rigidbody.velocity.z).magnitude;
            _anim.SetFloat(Define.Anim.Speed, currentSpeed, 0.1f, Time.fixedDeltaTime);
            
            float targetSpeed = _isSprinting ? _sprintSpeed : _moveSpeed;
            float motionSpeed = targetSpeed > 0 ? Mathf.Clamp(currentSpeed / targetSpeed, 0.5f, 1.5f) : 1f;
            _anim.SetFloat("MotionSpeed", motionSpeed);
            
            // Grounded 상태
            _anim.SetBool("Grounded", IsGrounded);
            
            // FreeFall (점프 안 하고 떨어질 때)
            if (!IsGrounded && _rigidbody.velocity.y < -2f)
            {
                _anim.SetTrigger("FreeFall");
            }
        }
        
        /// <summary>
        /// 점프 입력 상태 변경 처리
        /// InputManager의 OnJumpKeyDown(bool) 이벤트에서 호출
        /// </summary>
        /// <param name="isPressed">키가 눌렸으면 true, 떼졌으면 false</param>
        private void HandleJumpChange(bool isPressed)
        {
            // 키를 누른 순간에만 점프 신호를 켠다.
            // 떼는 순간(false)은 무시하거나, 필요하다면 _jumpInput = false로 설정할 수 있다.
            // 여기서는 JumpAndGravity 내부에서 소비(_jumpInput = false)하므로,
            // 누르는 신호(true)만 받아준다.
            if (isPressed)
            {
                _jumpInput = true;
            }
        }

        private void HandleSprintChange(bool isSprint)
        {
            _isSprinting = isSprint;
        }

        private void HandleMoveChange(Vector2 moveInput)
        {
            _currentMoveInput = moveInput;
        }
        
        protected override void OnLand()
        {
            base.OnLand();
            // 착지 사운드 등
        }

        protected override void OnFall()
        {
            base.OnFall();
        }
    }
}
