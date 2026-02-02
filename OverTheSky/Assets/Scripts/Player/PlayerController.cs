using System;
using OverTheSky.Core;
using OverTheSky.Gimmicks.Platforms;
using UnityEngine;
using Logger = OverTheSky.Core.Logger;

namespace OverTheSky.Player
{
    [RequireComponent(typeof(ForceReceiver))]
    public class PlayerController : PlayerBase
    {
        [Header("Movement Settings")] 
        [SerializeField] private float _moveSpeed = 2f;
        [SerializeField] [Range(0, 20)] private float _sprintSpeed = 6f;
        [SerializeField] private float _acceleration = 15f;
        [SerializeField] private float _rotationSpeed = 10f;

        [Header("Jump Settings")]
        [SerializeField] private float _jumpHeight = 1.2f;
        [SerializeField] private float _gravity = -15f;
        
        [Header("Air Control Settings")]
        [SerializeField] [Range(0f, 1f)] private float _airControlFactor = 0.3f;
        
        [Header("Debug")]
        [SerializeField] private bool _showDebugLog = false;
        
        // 물리 상태
        private float _verticalVelocity;
        private Vector3 _horizontalVelocity;
        private float _terminalVelocity = 53f;
        
        // 입력 상태
        private Vector2 _currentMoveInput;
        private bool _isSprinting;
        private Camera mainCamera;
        
        private ForceReceiver _forceReceiver; // 충격 리시버
        private IMovingPlatform _currentPlatform; // 밟고 있는 발판

        protected override void Awake()
        {
            base.Awake();
            mainCamera = Camera.main;
            // ForceReceiver 가져오기 (없으면 추가)
            _forceReceiver = GetComponent<ForceReceiver>();
            if (_forceReceiver == null) _forceReceiver = gameObject.AddComponent<ForceReceiver>();
            
            SetupFrictionlessMaterial();
        }
        
        private void SetupFrictionlessMaterial()
        {
            PhysicMaterial frictionless = new PhysicMaterial("PlayerFrictionless");
            frictionless.dynamicFriction = 0f;
            frictionless.staticFriction = 0f;
            frictionless.frictionCombine = PhysicMaterialCombine.Minimum;
            frictionless.bounceCombine = PhysicMaterialCombine.Minimum;
            frictionless.bounciness = 0f;
            
            if (_col != null)
            {
                _col.material = frictionless;
            }
        }
        
        protected override void FixedUpdate()
        {
            base.FixedUpdate();
            
            ReadInput();
            
            //_rigidbody.drag = 0f;
            
            ProcessJump();
            ApplyGravity();
            Move();
            UpdateAnimation();
        }
        
        // 발판 감지: 태그 비교 대신 인터페이스 유무로 판단
        private void OnCollisionEnter(Collision collision)
        {
            var platform = collision.gameObject.GetComponent<IMovingPlatform>();
            if (platform != null) _currentPlatform = platform;
        }
        
        private void OnCollisionStay(Collision collision)
        {
            // 혹시 Enter가 무시될 경우를 대비해 Stay에서도 체크
            if (_currentPlatform == null)
            {
                var platform = collision.gameObject.GetComponent<IMovingPlatform>();
                if (platform != null) _currentPlatform = platform;
            }
        }
        
        // 발판에서 내리면 해제
        private void OnCollisionExit(Collision collision)
        {
            var platform = collision.gameObject.GetComponent<IMovingPlatform>();
            if (platform == _currentPlatform) _currentPlatform = null;
        }

        public void Update()
        {
            UIManager.Instance?.UpdateUI(transform.position.y);
        }

        private void ReadInput()
        {
            if (InputManager.Instance == null) return;
            
            _currentMoveInput = InputManager.Instance.MoveInput;
            _isSprinting = InputManager.Instance.SprintKeyDown;
        }
        
        private void ApplyGravity()
        {
            if (IsGrounded && _verticalVelocity < 0f)
            {
                // 오를 수 있는 경사면 바닥이면 누르는 힘 0으로 미끄럼 방지
                // 평지면 -2f로 힘으로 눌러 판정성 안정화 
                _verticalVelocity = (SlopeAngle > 0f) ? 0f : -2f;
                return;
            }
            // 공중일 때 중력 누적
            if (_verticalVelocity > -_terminalVelocity)
            {
                _verticalVelocity += _gravity * Time.fixedDeltaTime;
            }
        }
        
        private void ProcessJump()
        {
            // 버퍼에서 점프 입력 소비
            bool jumpPressed = InputManager.Instance != null && InputManager.Instance.ConsumeJump();
            
            // 땅 or 가파른 경사
            bool canJump = IsGrounded || (SlopeAngle > 0f && SlopeAngle < 85f);
            
            if (canJump && jumpPressed && !IsCeiling)
            {
                // 수직 점프 (기본)
                _verticalVelocity = Mathf.Sqrt(_jumpHeight * -2f * _gravity);
                
                // 가파른 경사면(= !IsGrounded)에서 점프했다면? 
                // -> 벽 반대 방향으로 밀쳐낸다! (등반 방지)
                if (!IsGrounded)
                {
                    // GroundNormal은 PlayerBase에서 계산해둔 벽의 방향
                    Vector3 pushDir = new Vector3(GroundNormal.x, 0f, GroundNormal.z).normalized;
            
                    // 이동 속도만큼 벽 반대로 튕겨냄 (Horizontal Velocity 덮어쓰기)
                    _horizontalVelocity = pushDir * _moveSpeed;
                }
                
                _anim.SetTrigger(Define.Anim.IsJump);
                
                if (_showDebugLog) Debug.Log($"[Jump] Velocity: {_verticalVelocity}");
            }
        }
        
        private void Move()
        {
            float targetSpeed = _isSprinting ? _sprintSpeed : _moveSpeed;
            
            // 입력이 없을 때 처리 (안미끄러지게)
            if (_currentMoveInput.magnitude < 0.01f)
            {
                if (IsGrounded)
                {
                    // 경사면에서도 미끄러지지 않게 X, Z를 아예 0으로 고정
                    // Y는 -2f (ApplyGravity에서 설정한 바닥 부착용 값) 유지
                    _rigidbody.velocity = new Vector3(0f, _verticalVelocity, 0f); 
                    _horizontalVelocity = Vector3.zero; // 내부 변수도 초기화
                }
                else
                {
                    // 공중에서는 기존대로 감속 (관성 유지)
                    float deceleration = _acceleration * _airControlFactor;
                    _horizontalVelocity = Vector3.MoveTowards(_horizontalVelocity, Vector3.zero, deceleration * Time.fixedDeltaTime);
                }
                // return;
            }
            else
            {
                // 입력이 있을때
                Vector3 moveDirection = GetCameraRelativeMovement(_currentMoveInput);
                
                if (moveDirection.magnitude > 0.01f)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, _rotationSpeed * Time.fixedDeltaTime);
                }
                
                if (IsGrounded)
                {
                    moveDirection = Vector3.ProjectOnPlane(moveDirection, GroundNormal).normalized;
                }
                
                Vector3 targetVelocity = moveDirection * targetSpeed;
                float currentAcceleration = IsGrounded ? _acceleration : _acceleration * _airControlFactor;
                _horizontalVelocity = Vector3.MoveTowards(_horizontalVelocity, targetVelocity, currentAcceleration * Time.fixedDeltaTime);
            }
            // 최종 속도 적용 단계
            
            // 플레이어의 자체 이동 속도 (입력 + 중력)
            Vector3 playerVelocity = new Vector3(_horizontalVelocity.x, _verticalVelocity, _horizontalVelocity.z);
            
            // 발판의 속도 계산
            Vector3 platformVelocity = Vector3.zero;
            // 발판 위에 있다면, 플레이어 발 밑 위치의 속도를 받아옴
            if (_currentPlatform != null)
            {
                platformVelocity = _currentPlatform.GetVelocityAtPoint(transform.position);
            }
            
            // 내 이동 속도 + 수직 속도 + (외부 컴포넌트가 계산해준 외부 충격량)
            Vector3 finalVelocity = playerVelocity + platformVelocity + _forceReceiver.Movement;
            
            _rigidbody.velocity = finalVelocity;
            // _rigidbody.velocity = playerVelocity + platformVelocity + _impactVelocity;
            // _rigidbody.velocity = new Vector3(_horizontalVelocity.x, _verticalVelocity, _horizontalVelocity.z);
#if UNITY_EDITOR
            if (_showDebugLog)
            {
                Debug.Log($"[Move] HVel: {_horizontalVelocity.magnitude:F2}, VVel: {_verticalVelocity:F2}, Grounded: {IsGrounded}");
            }
#endif
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
        
        private void UpdateAnimation()
        {
            float currentSpeed = _horizontalVelocity.magnitude;
            _anim.SetFloat(Define.Anim.Speed, currentSpeed, 0.1f, Time.fixedDeltaTime);
            
            float targetSpeed = _isSprinting ? _sprintSpeed : _moveSpeed;
            float motionSpeed = targetSpeed > 0 ? Mathf.Clamp(currentSpeed / targetSpeed, 0.5f, 1.5f) : 1f;
            _anim.SetFloat(Define.Anim.MotionSpeed, motionSpeed);
            
            _anim.SetBool(Define.Anim.IsGrounded, IsGrounded);
            
            bool isFalling = !IsGrounded && _verticalVelocity < -5.0f; 
    
            _anim.SetBool(Define.Anim.IsFalling, isFalling);
        }
        
        protected override void OnLand()
        {
            base.OnLand();
            _anim.ResetTrigger(Define.Anim.IsJump);
        }

        protected override void OnFall()
        {
            base.OnFall();
        }
    }
}