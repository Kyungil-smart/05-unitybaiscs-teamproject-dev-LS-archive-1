using System;
using UnityEngine;

namespace OverTheSky.Core
{
    public class InputManager : Singleton<InputManager>
    {
        // 이동: (x, y) 벡터를 매개변수로 넘겨줌
        public event Action<Vector2> OnMove; 
        // 달리기: true/false
        public event Action<bool> OnSprintKeyDown;
        // 점프
        public event Action OnJump;
        
        // 프로퍼티 (값이 변할 때만 이벤트 호출)
        private Vector2 _moveInput;
        public Vector2 MoveInput
        {
            get => _moveInput;
            private set
            {
                if (_moveInput != value)
                {
                    _moveInput = value;
                    OnMove?.Invoke(value);
                }
            }
        }
        private bool _sprintKeyDown;
        public bool SprintKeyDown
        {
            get => _sprintKeyDown;
            private set
            {
                if (_sprintKeyDown != value)
                {
                    _sprintKeyDown = value;
                    OnSprintKeyDown?.Invoke(value);
                }
            }
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
            float h = Input.GetAxisRaw("Horizontal");
            float v = Input.GetAxisRaw("Vertical");
            
            MoveInput = new Vector2(h, v).normalized;
            
            SprintKeyDown = Input.GetKey(KeyCode.LeftShift);
            
            if (Input.GetKeyDown(KeyCode.Space)) OnJump?.Invoke();
        }
        
        public void SetInputActive(bool active)
        {
            _isInputBlocked = !active;
            if (_isInputBlocked)
            {
                // 차단될 때 모든 입력 값을 0으로
                MoveInput = Vector2.zero; 
                SprintKeyDown = false;
            }
        }
    }
}
