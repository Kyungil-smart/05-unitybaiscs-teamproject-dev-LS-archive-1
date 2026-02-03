using UnityEngine;

namespace OverTheSky.CameraComponents
{
    /// <summary>
    /// 3인칭 카메라 컨트롤러
    /// - 마우스로 카메라 회전
    /// - 타겟(플레이어) 추적
    /// - 부드러운 이동
    /// </summary>
    public class CameraController : MonoBehaviour
    {
        #region Serialized Fields
        
        [Header("Target Settings")]
        [Tooltip("따라갈 타겟 (플레이어 Transform)")]
        [SerializeField] private Transform _target;
        
        [Header("Distance Settings")]
        [Tooltip("카메라와 타겟 사이 거리")]
        [SerializeField] private float _distance = 5f;
        
        [Tooltip("카메라 높이 오프셋")]
        [SerializeField] private float _height = 2f;
        
        [Tooltip("카메라 따라가는 부드러움 (높을수록 빠름)")]
        [SerializeField] private float _smoothSpeed = 10f;
        
        [Header("Rotation Settings")]
        [Tooltip("마우스 감도 (좌우/상하)")]
        [SerializeField] private Vector2 _mouseSensitivity = new Vector2(2f, 2f);
        
        [Tooltip("상하 회전 제한 - 위쪽 각도")]
        [SerializeField] private float _topClamp = 70f;
        
        [Tooltip("상하 회전 제한 - 아래쪽 각도")]
        [SerializeField] private float _bottomClamp = -30f;
        
        [Header("Zoom Settings")]
        [Tooltip("줌 기능 사용 여부")]
        [SerializeField] private bool _enableZoom = true;
        
        [Tooltip("최소 거리 (가장 가까이)")]
        [SerializeField] private float _minDistance = 2f;
        
        [Tooltip("최대 거리 (가장 멀리)")]
        [SerializeField] private float _maxDistance = 10f;
        
        [Tooltip("줌 속도")]
        [SerializeField] private float _zoomSpeed = 2f;
        
        [Header("Collision Settings")]
        [Tooltip("카메라 충돌 감지 여부")]
        [SerializeField] private bool _enableCollision = true;
        
        [Tooltip("충돌 체크할 레이어")]
        [SerializeField] private LayerMask _collisionLayers = -1;
        
        [Tooltip("카메라 반지름 (충돌 감지용)")]
        [SerializeField] private float _cameraRadius = 0.2f;
        
        #endregion
        
        #region Private Fields
        
        // 카메라 회전 각도
        private float _yaw;     // 좌우 회전 (Y축)
        private float _pitch;   // 상하 회전 (X축)
        
        // 현재 거리 (줌 적용)
        private float _currentDistance;
        
        // 충돌 보정된 거리
        private float _collisionDistance;
        
        #endregion
        
        #region Unity Lifecycle
        
        private void Start()
        {
            // 초기 회전값 설정 (현재 카메라 각도)
            Vector3 angles = transform.eulerAngles;
            _yaw = angles.y;
            _pitch = angles.x;
            
            // 각도 정규화 (-180 ~ 180)
            if (_pitch > 180f) _pitch -= 360f;
            
            // 현재 거리 초기화
            _currentDistance = _distance;
            _collisionDistance = _distance;
        }
        
        private void LateUpdate()
        {
            // LateUpdate: 플레이어 이동 후 카메라 업데이트
            
            if (_target == null)
            {
                Debug.LogWarning("CameraController: Target이 설정되지 않았습니다!");
                return;
            }
            
            // 마우스 입력 처리
            HandleRotationInput();
            
            // 줌 입력 처리
            if (_enableZoom)
            {
                HandleZoomInput();
            }
            
            // 카메라 위치/회전 업데이트
            UpdateCameraTransform();
        }
        
        #endregion
        
        #region Input Handling
        
        /// <summary>
        /// 마우스 입력으로 카메라 회전 처리
        /// </summary>
        private void HandleRotationInput()
        {
            // 마우스 입력 받기
            float mouseX = Input.GetAxis("Mouse X") * _mouseSensitivity.x;
            float mouseY = Input.GetAxis("Mouse Y") * _mouseSensitivity.y;
            
            // 회전 각도 업데이트
            _yaw += mouseX;
            _pitch -= mouseY;  // Y축은 반대 (위로 움직이면 각도 감소)
            
            // 상하 각도 제한 (하늘/땅 너무 안 보이게)
            _pitch = Mathf.Clamp(_pitch, _bottomClamp, _topClamp);
        }
        
        /// <summary>
        /// 마우스 휠로 줌 처리
        /// </summary>
        private void HandleZoomInput()
        {
            // 마우스 휠 입력
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            
            if (Mathf.Abs(scroll) > 0.01f)
            {
                // 거리 조절 (휠 위 = 가까이, 휠 아래 = 멀리)
                _currentDistance -= scroll * _zoomSpeed;
                
                // 최소/최대 거리 제한
                _currentDistance = Mathf.Clamp(_currentDistance, _minDistance, _maxDistance);
            }
        }
        
        #endregion
        
        #region Camera Transform
        
        /// <summary>
        /// 카메라 위치와 회전 업데이트
        /// </summary>
        private void UpdateCameraTransform()
        {
            // 타겟 위치 (플레이어 위치 + 높이 오프셋)
            Vector3 targetPosition = _target.position + Vector3.up * _height;
            
            // 회전 계산 (마우스 입력이 반영된 Pitch, Yaw 적용)_회전 쿼터니언 생성
            Quaternion rotation = Quaternion.Euler(_pitch, _yaw, 0f);
            
            // 카메라 오프셋 계산 (타겟 뒤쪽)
            Vector3 offset = rotation * new Vector3(0f, 0f, -_currentDistance);
            
            // 충돌 체크 및 거리 보정
            float finalDistance = _currentDistance;
            if (_enableCollision)
            {
                // 플레이어에서 카메라 방향으로 SphereCast 발사
                // 장애물 감지 시 finalDistance를 충돌 지점까지로 단축
                finalDistance = CheckCameraCollision(targetPosition, offset.normalized, _currentDistance);
            }
            
            // 부드러운 거리 적용 (갑자기 튀는 현상 방지)
            _collisionDistance = Mathf.Lerp(_collisionDistance, finalDistance, _smoothSpeed * Time.deltaTime);
            
            // 최종 오프셋 (충돌 보정 적용)
            Vector3 finalOffset = rotation * new Vector3(0f, 0f, -_collisionDistance);
            
            // 최종 카메라 위치
            Vector3 desiredPosition = targetPosition + finalOffset;
            
            // 부드럽게 이동 및 LookAt
            transform.position = Vector3.Lerp(
                transform.position, 
                desiredPosition, 
                _smoothSpeed * Time.deltaTime
            );
            
            // 타겟 바라보기
            transform.LookAt(targetPosition);
        }
        
        /// <summary>
        /// 카메라 충돌 체크
        /// 벽에 막혔을 때 카메라를 가까이 당김
        /// </summary>
        /// <param name="origin">시작 위치 (타겟)</param>
        /// <param name="direction">방향 (카메라 쪽)</param>
        /// <param name="maxDistance">최대 거리</param>
        /// <returns>충돌 보정된 거리</returns>
        private float CheckCameraCollision(Vector3 origin, Vector3 direction, float maxDistance)
        {
            // SphereCast로 충돌 체크
            if (Physics.SphereCast(
                origin,                     // 시작점 (플레이어)
                _cameraRadius,              // 구 반지름
                direction,                  // 방향 (카메라 쪽)
                out RaycastHit hit,         // 충돌 정보
                maxDistance,                // 최대 거리
                _collisionLayers,           // 충돌 레이어
                QueryTriggerInteraction.Ignore))
            {
                // 충돌 지점까지의 거리 반환
                // 약간 여유를 둬서 벽에 딱 붙지 않게
                return Mathf.Max(hit.distance - _cameraRadius, _minDistance);
            }
            
            // 충돌 없으면 원래 거리
            return maxDistance;
        }
        
        #endregion
        
        #region Public Methods
        
        /// <summary>
        /// 타겟 설정 (런타임에서 변경 가능)
        /// </summary>
        public void SetTarget(Transform newTarget)
        {
            _target = newTarget;
        }
        
        /// <summary>
        /// 카메라 거리 설정
        /// </summary>
        public void SetDistance(float distance)
        {
            _currentDistance = Mathf.Clamp(distance, _minDistance, _maxDistance);
        }
        
        /// <summary>
        /// 카메라 각도 설정 (즉시)
        /// </summary>
        public void SetRotation(float yaw, float pitch)
        {
            _yaw = yaw;
            _pitch = Mathf.Clamp(pitch, _bottomClamp, _topClamp);
        }
        
        /// <summary>
        /// 카메라 각도 가져오기
        /// </summary>
        public Vector2 GetRotation()
        {
            return new Vector2(_yaw, _pitch);
        }
        
        #endregion
        
        #region Gizmos
        
        private void OnDrawGizmosSelected()
        {
            if (_target == null) return;
            
            // 타겟 위치
            Vector3 targetPos = _target.position + Vector3.up * _height;
            
            // 카메라 충돌 범위 표시
            if (_enableCollision)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(targetPos, _cameraRadius);
                
                // 카메라 방향 선
                Vector3 direction = (transform.position - targetPos).normalized;
                Gizmos.DrawLine(targetPos, targetPos + direction * _currentDistance);
            }
            
            // 최소/최대 거리 범위
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(targetPos, _minDistance);
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(targetPos, _maxDistance);
        }
        
        #endregion
    }
}