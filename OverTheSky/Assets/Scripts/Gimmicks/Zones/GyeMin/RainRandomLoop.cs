using System.Collections;
using System.Reflection;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(ParticleSystem))]
public class RainRandomLoop : MonoBehaviour
{
    [Header("비 연출 제어(매니저용)")]
    [SerializeField] private Transform particleRoot;           // 비 파티클 루트(없으면 자기 오브젝트)
    [SerializeField] private Vector2 activeDurationRange = new Vector2(6f, 12f); // 비 지속시간
    [SerializeField] private bool stopOnAwake = true;

    private ParticleSystem[] allPS;

    [Header("느려질 대상(플레이어) 태그")]
    [Tooltip("플레이어 루트(ThirdPersonRigidbodyController가 붙은 오브젝트)의 태그. 비우면 태그 검사 안 함.")]
    [SerializeField] private string targetTag = "Player";

    [Header("느려짐 설정")]
    [Range(0.1f, 1f)]
    [Tooltip("0.7이면 30% 느려짐")]
    [SerializeField] private float slowMultiplier = 0.7f;

    [Tooltip("마지막으로 비를 맞은 뒤 느려짐 유지 시간(초)")]
    [SerializeField] private float slowDuration = 2f;

    [Tooltip("runSpeed도 같이 느리게 할지(권장 ON)")]
    [SerializeField] private bool slowRunSpeedToo = true;

    [Header("디버그")]
    [Tooltip("켜면 비가 맞을 때마다 로그를 출력합니다.")]
    [SerializeField] private bool logOnHit = false;

    [Tooltip("테스트용: 이 키를 누르면 강제로 슬로우를 한 번 적용합니다.")]
    [SerializeField] private KeyCode testKey = KeyCode.F8;

    private ThirdPersonRigidbodyController ctrl;
    private FieldInfo walkField;
    private FieldInfo runField;

    private float baseWalk;
    private float baseRun;

    private bool slowed;
    private float recoverAt;

    private static readonly BindingFlags FLAGS = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

    void Update()
    {
        // 강제 테스트(파티클 충돌이 안 올 때, 바인딩/감속이 되는지 먼저 확인 가능)
        if (Input.GetKeyDown(testKey))
        {
            var anyPlayer = FindObjectOfType<ThirdPersonRigidbodyController>();
            if (anyPlayer != null)
            {
                BindIfNeeded(anyPlayer);
                ApplySlowAndRefreshTimer();
                if (logOnHit) Debug.Log("[RainRandomLoop] 테스트 키로 슬로우 적용");
            }
            else
            {
                Debug.LogWarning("[RainRandomLoop] 테스트: ThirdPersonRigidbodyController를 씬에서 못 찾음");
            }
        }

        // 타이머 방식 복구
        if (slowed && Time.time >= recoverAt)
            Restore();
    }

    void OnParticleCollision(GameObject other)
    {
        if (other == null) return;

        // 맞은 대상에서 컨트롤러 찾기 
        var hitCtrl = other.GetComponentInParent<ThirdPersonRigidbodyController>();
        if (hitCtrl == null)
            hitCtrl = other.GetComponentInChildren<ThirdPersonRigidbodyController>(true);

        if (hitCtrl == null)
        {
            if (logOnHit) Debug.Log($"[RainRandomLoop] Hit: {other.name} (컨트롤러 없음)");
            return;
        }

        // 태그 체크는 "컨트롤러가 붙은 루트" 기준으로
        if (!string.IsNullOrEmpty(targetTag) && !hitCtrl.CompareTag(targetTag))
        {
            if (logOnHit) Debug.Log($"[RainRandomLoop] HitCtrl={hitCtrl.name} 태그 불일치({hitCtrl.tag})");
            return;
        }

        BindIfNeeded(hitCtrl);
        ApplySlowAndRefreshTimer();

        if (logOnHit)
        {
            float w = (float)walkField.GetValue(ctrl);
            float r = runField != null ? (float)runField.GetValue(ctrl) : -1f;
            Debug.Log($"[RainRandomLoop] HIT! walk={w:0.###}, run={(r < 0 ? "N/A" : r.ToString("0.###"))}");
        }
    }

    private void BindIfNeeded(ThirdPersonRigidbodyController target)
    {
        if (ctrl == target && walkField != null) return;

        ctrl = target;

        var t = ctrl.GetType();
        walkField = t.GetField("walkSpeed", FLAGS);
        runField  = t.GetField("runSpeed",  FLAGS);

        if (walkField == null)
        {
            Debug.LogError("[RainRandomLoop] ThirdPersonRigidbodyController에서 walkSpeed 필드를 찾지 못했습니다.\n" +
                           "MovePlayer.cs에서 변수명이 바뀌었는지 확인하세요.");
            enabled = false;
            return;
        }

        // 기준값 저장
        baseWalk = (float)walkField.GetValue(ctrl);
        if (runField != null) baseRun = (float)runField.GetValue(ctrl);

        slowed = false;
        recoverAt = 0f;
    }

    private void ApplySlowAndRefreshTimer()
    {
        if (ctrl == null || walkField == null) return;

        // 처음 느려질 때만 현재 값을 기준으로 저장(인게임에서 값이 바뀌어도 대응)
        if (!slowed)
        {
            baseWalk = (float)walkField.GetValue(ctrl);
            if (slowRunSpeedToo && runField != null)
                baseRun = (float)runField.GetValue(ctrl);
        }

        slowed = true;
        recoverAt = Time.time + Mathf.Max(0.01f, slowDuration);

        // 감속 적용
        walkField.SetValue(ctrl, baseWalk * slowMultiplier);
        if (slowRunSpeedToo && runField != null)
            runField.SetValue(ctrl, baseRun * slowMultiplier);
    }

    private void Restore()
    {
        if (ctrl == null || walkField == null) { slowed = false; return; }

        walkField.SetValue(ctrl, baseWalk);
        if (slowRunSpeedToo && runField != null)
            runField.SetValue(ctrl, baseRun);

        slowed = false;
    }

    private void Awake()
    {
        if (particleRoot == null) particleRoot = transform;
        CacheParticles();
        if (stopOnAwake) ForceStop();
    }

    private void CacheParticles()
    {
        if (particleRoot == null) return;
        allPS = particleRoot.GetComponentsInChildren<ParticleSystem>(true);
    }

    // 매니저가 호출: 비 1회 연출
    public IEnumerator PlayOnce()
    {
        if (particleRoot == null) particleRoot = transform;
        if (allPS == null || allPS.Length == 0) CacheParticles();

        for (int i = 0; i < allPS.Length; i++)
            if (allPS[i] != null) allPS[i].Play(true);

        yield return new WaitForSeconds(RandomRange(activeDurationRange));

        ForceStop();
    }

    // 매니저가 호출: 즉시 종료(겹침 방지)
    public void ForceStop()
    {
        if (particleRoot == null) particleRoot = transform;
        if (allPS == null || allPS.Length == 0) CacheParticles();

        for (int i = 0; i < allPS.Length; i++)
            if (allPS[i] != null) allPS[i].Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
    }

    private float RandomRange(Vector2 range)
    {
        float a = Mathf.Min(range.x, range.y);
        float b = Mathf.Max(range.x, range.y);
        return Random.Range(a, b);
    }

}
