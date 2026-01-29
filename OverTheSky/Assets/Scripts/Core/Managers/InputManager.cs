using System;
using UnityEngine;

namespace OverTheSky.Core
{
    public class InputManager : Singleton<InputManager>
    {
        public event Action<Vector2> OnMove; 
        public event Action<bool> OnSprintKeyDown;
        public event Action<bool> OnJumpKeyDown;
        
        // 현재 입력 상태
        public Vector2 MoveInput { get; private set; }
        public bool SprintKeyDown { get; private set; }
        
        // 점프 입력 버퍼 - FixedUpdate에서 소비할 때까지 유지
        private bool _jumpBuffered = false;
        public bool ConsumeJump()
        {
            if (_jumpBuffered)
            {
                _jumpBuffered = false;
                return true;
            }
            return false;
        }
        
        private bool _isInputBlocked = false;
        
        protected override void Awake()
        {
            base.Awake();
        }
        
        private void Update()
        {
            if (_isInputBlocked) return;
            ProcessInputs();
        }
        
        private void ProcessInputs()
        {
            // 이동 입력
            float h = Input.GetAxisRaw("Horizontal");
            float v = Input.GetAxisRaw("Vertical");
            MoveInput = new Vector2(h, v).normalized;
            
            // 달리기 입력
            SprintKeyDown = Input.GetKey(KeyCode.LeftShift);
            
            // 점프: GetKeyDown이 true면 버퍼에 저장 (소비될 때까지 유지)
            if (Input.GetKeyDown(KeyCode.Space))
            {
                _jumpBuffered = true;
            }
        }
        
        public void SetInputActive(bool active)
        {
            _isInputBlocked = !active;
            if (_isInputBlocked)
            {
                MoveInput = Vector2.zero; 
                SprintKeyDown = false;
                _jumpBuffered = false;
            }
        }
    }
}