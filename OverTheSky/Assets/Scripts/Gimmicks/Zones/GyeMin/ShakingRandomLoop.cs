using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 카메라(로컬 오프셋) + 맵(리지드바디) 지진 효과를 "랜덤 루프"로 재생
public class ShakingRandomLoop : MonoBehaviour
{
    [Header("대상: 카메라(선택)")]
    [Tooltip("카메라 흔들림을 적용할 Transform. 추천: 카메라 리그 아래에 'CameraShakePivot' 같은 자식 오브젝트를 하나 만들어 거기에 적용.")]
    [SerializeField] private Transform cameraShakeTarget;

    [Tooltip("카메라 위치 흔들림 사용")]
    [SerializeField] private bool shakeCameraPosition = true;

    [Tooltip("카메라 회전 흔들림 사용")]
    [SerializeField] private bool shakeCameraRotation = true;

    [Header("대상: 맵/바닥 Rigidbody(선택)")]
    [Tooltip("여기에 바닥/플랫폼 Rigidbody들을 직접 넣거나, 아래 Root를 지정하고 자동 수집을 켜세요.")]
    [SerializeField] private List<Rigidbody> mapRigidbodies = new List<Rigidbody>();

    [Tooltip("Root 하위 Rigidbody들을 자동으로 수집합니다.")]
    [SerializeField] private Transform mapRigidbodyRoot;

    [SerializeField] private bool autoCollectFromRoot = true;

    [Tooltip("자동 수집 시 비활성 오브젝트 포함")]
    [SerializeField] private bool includeInactive = true;

    [Header("루프: 게임 시작 후 '첫 실행' 대기(랜덤)")]
    [SerializeField] private Vector2 firstDelayRange = new Vector2(5f, 12f);

    [Header("루프: 한 번 끝난 뒤 '다음 실행'까지 대기(랜덤)")]
    [SerializeField] private Vector2 intervalRange = new Vector2(10f, 25f);

    [Header("지진 연출 시간(랜덤) - FadeIn / Hold / FadeOut")]
    [SerializeField] private Vector2 fadeInRange = new Vector2(0.15f, 0.35f);
    [SerializeField] private Vector2 holdRange = new Vector2(1.0f, 2.5f);
    [SerializeField] private Vector2 fadeOutRange = new Vector2(0.15f, 0.45f);

    [Header("지진 강도(랜덤)")]
    [Tooltip("0~1 범위. 1이면 최대치(아래 카메라/맵 최대값 그대로).")]
    [SerializeField] private Vector2 peakIntensityRange = new Vector2(0.6f, 1.0f);

    [Header("카메라: 최대 흔들림(강도 1 기준)")]
    [SerializeField] private float cameraMaxPosOffset = 0.15f; 
    [SerializeField] private float cameraMaxRotAngle = 1.5f;  

    [Header("카메라: 흔들림 속도(랜덤)")]
    [SerializeField] private Vector2 cameraFrequencyRange = new Vector2(12f, 20f);

    public enum MapShakeMode
    {
        Auto,          
        VelocityOnly,   
        MovePositionOnly 
    }

    [Header("맵: 흔들기 방식")]
    [SerializeField] private MapShakeMode mapShakeMode = MapShakeMode.Auto;

    [Header("맵: 방향")]
    [SerializeField] private bool mapShakeX = true;
    [SerializeField] private bool mapShakeZ = false;

    [Header("맵: 속도/이동량(강도 1 기준)")]
    [Tooltip("Non-kinematic Rigidbody에 적용할 velocity 진폭(m/s).")]
    [SerializeField] private float mapVelocityAmount = 1.0f;

    [Tooltip("Kinematic Rigidbody에 적용할 MovePosition 이동 진폭(m).")]
    [SerializeField] private float mapMoveDistance = 0.05f;

    [Header("맵: 흔들림 속도(랜덤)")]
    [SerializeField] private Vector2 mapShakeSpeedRange = new Vector2(1.0f, 3.0f);

    [Header("옵션")]
    [SerializeField] private bool autoPlayOnStart = true;

    private Coroutine loopCo;

    private bool quakeActive;
    private float quakePeakIntensity;
    private float quakeCameraFrequency;
    private float quakeMapSpeed;
    private float quakeSeed;

    private float currentIntensity; 

    private Vector3 camLastPosOffset;
    private Quaternion camLastRotOffset = Quaternion.identity;

    private readonly Dictionary<Rigidbody, Vector3> rbStartPos = new Dictionary<Rigidbody, Vector3>();
    private readonly Dictionary<Rigidbody, Quaternion> rbStartRot = new Dictionary<Rigidbody, Quaternion>();
    private readonly Dictionary<Rigidbody, Vector3> rbStartVel = new Dictionary<Rigidbody, Vector3>();
    private readonly Dictionary<Rigidbody, Vector3> rbStartAngVel = new Dictionary<Rigidbody, Vector3>();

    private void Start()
    {
        if (cameraShakeTarget == null && Camera.main != null)
            cameraShakeTarget = Camera.main.transform;

        if (autoCollectFromRoot)
            CollectMapRigidbodies();

        if (autoPlayOnStart)
            loopCo = StartCoroutine(Loop());
    }

    private void OnDisable()
    {
        StopAllCoroutines();
        EndQuakeImmediate();
    }

    // Public Controls
    [ContextMenu("Trigger Quake Now (Random Settings)")]
    public void TriggerQuakeNow()
    {
        StartCoroutine(PlayOneQuake());
    }

    public void StopLoop()
    {
        if (loopCo != null)
        {
            StopCoroutine(loopCo);
            loopCo = null;
        }
        EndQuakeImmediate();
    }
    private IEnumerator Loop()
    {
        // 첫 실행까지 랜덤 대기
        yield return new WaitForSeconds(RandomRange(firstDelayRange));

        while (true)
        {
            yield return PlayOneQuake();

            // 다음 실행까지 랜덤 대기
            yield return new WaitForSeconds(RandomRange(intervalRange));
        }
    }

    private IEnumerator PlayOneQuake()
    {
        float fadeIn = RandomRange(fadeInRange);
        float hold = RandomRange(holdRange);
        float fadeOut = RandomRange(fadeOutRange);
        float peak = Mathf.Clamp01(RandomRange(peakIntensityRange));

        float camFreq = RandomRange(cameraFrequencyRange);
        float mapSpeed = RandomRange(mapShakeSpeedRange);

        BeginQuake(peak, camFreq, mapSpeed);

        // Fade In
        yield return FadeIntensity(0f, peak, fadeIn);

        // Hold
        yield return HoldIntensity(peak, hold);

        // Fade Out
        yield return FadeIntensity(peak, 0f, fadeOut);

        EndQuakeImmediate();
    }

    private void BeginQuake(float peak, float cameraFreq, float mapSpeed)
    {
        quakeActive = true;
        quakePeakIntensity = peak;
        quakeCameraFrequency = Mathf.Max(1f, cameraFreq);
        quakeMapSpeed = Mathf.Max(0.01f, mapSpeed);
        quakeSeed = Random.Range(-10000f, 10000f);

        CacheMapStates();
    }

    private IEnumerator FadeIntensity(float from, float to, float seconds)
    {
        if (seconds <= 0f)
        {
            currentIntensity = to;
            yield break;
        }

        float t = 0f;
        while (t < seconds)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / seconds);
            currentIntensity = Mathf.Lerp(from, to, k);
            yield return null;
        }

        currentIntensity = to;
    }

    private IEnumerator HoldIntensity(float v, float seconds)
    {
        currentIntensity = v;
        if (seconds <= 0f) yield break;

        float t = 0f;
        while (t < seconds)
        {
            t += Time.deltaTime;
            yield return null;
        }
    }

    private void EndQuakeImmediate()
    {
        quakeActive = false;
        currentIntensity = 0f;

        // 카메라 오프셋 복구
        RemoveCameraLastOffset();

        // 맵 복구
        RestoreMapStates();
    }

    // Camera Shake (LateUpdate)
    private void LateUpdate()
    {
        if (cameraShakeTarget == null) return;

        // 매 프레임 "이전 오프셋 제거" → 누적 방지
        RemoveCameraLastOffset();

        if (!quakeActive || currentIntensity <= 0f) return;

        float amp = Mathf.Clamp01(currentIntensity);

        // Perlin 기반 (부드러운 지진 느낌)
        float t = Time.time * quakeCameraFrequency;
        float nx = (Mathf.PerlinNoise(t + quakeSeed, 0.17f) - 0.5f) * 2f;
        float ny = (Mathf.PerlinNoise(0.33f, t + quakeSeed) - 0.5f) * 2f;
        float nr = (Mathf.PerlinNoise(t + quakeSeed, t + 0.77f) - 0.5f) * 2f;

        if (shakeCameraPosition)
        {
            camLastPosOffset = new Vector3(nx, ny, 0f) * (cameraMaxPosOffset * amp);
            cameraShakeTarget.localPosition += camLastPosOffset;
        }

        if (shakeCameraRotation)
        {
            camLastRotOffset = Quaternion.Euler(0f, 0f, nr * cameraMaxRotAngle * amp);
            cameraShakeTarget.localRotation = cameraShakeTarget.localRotation * camLastRotOffset;
        }
    }

    private void RemoveCameraLastOffset()
    {
        if (cameraShakeTarget == null) return;

        if (camLastPosOffset != Vector3.zero)
        {
            cameraShakeTarget.localPosition -= camLastPosOffset;
            camLastPosOffset = Vector3.zero;
        }

        if (camLastRotOffset != Quaternion.identity)
        {
            cameraShakeTarget.localRotation = cameraShakeTarget.localRotation * Quaternion.Inverse(camLastRotOffset);
            camLastRotOffset = Quaternion.identity;
        }
    }

    // Map Shake (FixedUpdate)
    private void FixedUpdate()
    {
        if (!quakeActive || currentIntensity <= 0f) return;
        if (mapRigidbodies == null || mapRigidbodies.Count == 0) return;

        float amp = Mathf.Clamp01(currentIntensity);

        float phase = (Time.time + quakeSeed) * 2f * Mathf.PI * quakeMapSpeed;

        float vx = Mathf.Cos(phase) * (mapVelocityAmount * amp);
        float vz = Mathf.Sin(phase) * (mapVelocityAmount * amp);

        float dx = Mathf.Sin(phase) * (mapMoveDistance * amp);
        float dz = Mathf.Cos(phase) * (mapMoveDistance * amp);

        Vector3 vel = new Vector3(mapShakeX ? vx : 0f, 0f, mapShakeZ ? vz : 0f);
        Vector3 posOffset = new Vector3(mapShakeX ? dx : 0f, 0f, mapShakeZ ? dz : 0f);

        for (int i = 0; i < mapRigidbodies.Count; i++)
        {
            var rb = mapRigidbodies[i];
            if (rb == null) continue;

            MapShakeMode mode = mapShakeMode;
            if (mode == MapShakeMode.Auto)
                mode = rb.isKinematic ? MapShakeMode.MovePositionOnly : MapShakeMode.VelocityOnly;

            if (mode == MapShakeMode.MovePositionOnly)
            {
                if (!rbStartPos.TryGetValue(rb, out var startPos)) startPos = rb.position;
                rb.MovePosition(startPos + posOffset);
            }
            else
            {
                if (!rbStartVel.TryGetValue(rb, out var baseVel)) baseVel = Vector3.zero;
                rb.velocity = baseVel + vel;
            }
        }
    }

    private void CollectMapRigidbodies()
    {
        if (mapRigidbodyRoot == null) return;

        var rbs = mapRigidbodyRoot.GetComponentsInChildren<Rigidbody>(includeInactive);
        for (int i = 0; i < rbs.Length; i++)
        {
            var rb = rbs[i];
            if (rb == null) continue;
            if (!mapRigidbodies.Contains(rb))
                mapRigidbodies.Add(rb);
        }
    }

    private void CacheMapStates()
    {
        rbStartPos.Clear();
        rbStartRot.Clear();
        rbStartVel.Clear();
        rbStartAngVel.Clear();

        if (mapRigidbodies == null) return;

        for (int i = 0; i < mapRigidbodies.Count; i++)
        {
            var rb = mapRigidbodies[i];
            if (rb == null) continue;

            rbStartPos[rb] = rb.position;
            rbStartRot[rb] = rb.rotation;
            rbStartVel[rb] = rb.velocity;
            rbStartAngVel[rb] = rb.angularVelocity;
        }
    }

    private void RestoreMapStates()
    {
        if (mapRigidbodies == null) return;

        for (int i = 0; i < mapRigidbodies.Count; i++)
        {
            var rb = mapRigidbodies[i];
            if (rb == null) continue;

            // 흔들림이 끝났을 때 "원상복구"
            if (rbStartPos.TryGetValue(rb, out var p) && rb.isKinematic)
                rb.MovePosition(p);

            if (rbStartVel.TryGetValue(rb, out var v))
                rb.velocity = v;

            if (rbStartAngVel.TryGetValue(rb, out var av))
                rb.angularVelocity = av;
        }
    }

    private float RandomRange(Vector2 range)
    {
        float a = Mathf.Min(range.x, range.y);
        float b = Mathf.Max(range.x, range.y);
        return Random.Range(a, b);
    }

    // 매니저가 호출: 1회 흔들림
    public IEnumerator PlayOnce()
    {
        StopLoop();               // 겹침 방지
        yield return PlayOneQuake();
    }

}

