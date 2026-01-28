using System.Collections.Generic;
using System.Collections;
using UnityEngine;

public class FogRandomLoop : MonoBehaviour
{
    [Header("필수: 네 안개 프리팹(Foz_Zone) 넣기")]
    [SerializeField] private GameObject fogPrefab;

    [Header("안개를 둘 위치(선택)")]
    [Tooltip("비워두면 이 스크립트가 붙은 오브젝트 위치에 생성됩니다.")]
    [SerializeField] private Transform spawnAnchor;

    [Tooltip("Anchor 기준 추가 오프셋(예: 바닥에 살짝 띄우기)")]
    [SerializeField] private Vector3 offset = new Vector3(0f, 0.05f, 0f);

    [Header("따라다니게 할지(선택)")]
    [Tooltip("켜면 안개가 spawnAnchor를 따라다닙니다.")]
    [SerializeField] private bool followAnchorWhileActive = true;

    [Tooltip("따라다니는 갱신 주기(초). 0이면 매 프레임.")]
    [SerializeField] private float followUpdateInterval = 0.1f;

    [Header("게임 시작 후 '첫 실행' 대기 시간(랜덤)")]
    [SerializeField] private Vector2 firstDelayRange = new Vector2(5f, 12f);

    [Header("한 번 끝난 뒤 '다음 실행'까지 대기 시간(랜덤)")]
    [SerializeField] private Vector2 intervalRange = new Vector2(10f, 25f);

    [Header("안개 연출 시간(랜덤)")]
    [SerializeField] private Vector2 fadeInRange = new Vector2(2f, 4f);
    [SerializeField] private Vector2 holdRange = new Vector2(3f, 7f);
    [SerializeField] private Vector2 fadeOutRange = new Vector2(2f, 4f);

    [Header("안개 최대 강도(선택)")]
    [Tooltip("1이면 프리팹의 원래 세기(Emission/색상) 그대로. 0.8이면 조금 약하게.")]
    [SerializeField] private Vector2 peakIntensityRange = new Vector2(1f, 1f);

    [Header("성능/안정 옵션")]
    [Tooltip("켜면 Rate over Distance도 같이 스케일합니다. (시선/위치 이동하는 안개면 보통 OFF 권장)")]
    [SerializeField] private bool scaleRateOverDistance = false;

    private GameObject fogInstance;
    private ParticleSystem ps;
    private ParticleSystemRenderer psRenderer;
    private ParticleSystem.MainModule main;
    private ParticleSystem.EmissionModule emission;

    private ParticleSystem.MinMaxCurve baseRateOverTime;
    private ParticleSystem.MinMaxCurve baseRateOverDistance;

    private Color baseStartColor;
    private bool hasConstantStartColor;

    private MaterialPropertyBlock mpb;
    private int colorId;
    private int tintColorId;

    private Coroutine loopCo;

    private void Start()
    {
        if (fogPrefab == null)
        {
            Debug.LogError("[FogRandomLoop] fogPrefab이 비어있습니다. Inspector에 Foz_Zone 프리팹을 넣어주세요.");
            return;
        }

        CreateOrReuseInstance();
        StopAndClear();

        loopCo = StartCoroutine(Loop());
    }

    private void CreateOrReuseInstance()
    {
        if (fogInstance != null) return;

        fogInstance = Instantiate(fogPrefab);
        fogInstance.name = fogPrefab.name + "_Instance";

        // 프리팹 루트/자식 어디에 있든 첫 ParticleSystem을 잡음
        ps = fogInstance.GetComponentInChildren<ParticleSystem>(true);
        psRenderer = fogInstance.GetComponentInChildren<ParticleSystemRenderer>(true);

        if (ps == null)
        {
            Debug.LogError("[FogRandomLoop] 프리팹 안에서 ParticleSystem을 찾지 못했습니다.");
            return;
        }

        main = ps.main;
        emission = ps.emission;

        // 현재 프리팹 값들을 "기준"으로 저장
        baseRateOverTime = emission.rateOverTime;
        baseRateOverDistance = emission.rateOverDistance;

        // StartColor가 상수(Color)일 때만 알파를 곱해 컨트롤
        var sc = main.startColor;
        hasConstantStartColor = sc.mode == ParticleSystemGradientMode.Color;
        baseStartColor = hasConstantStartColor ? sc.color : Color.white;

        // 머티리얼 알파도 컨트롤(가능한 셰이더면)
        mpb = new MaterialPropertyBlock();
        colorId = Shader.PropertyToID("_Color");
        tintColorId = Shader.PropertyToID("_TintColor");
    }

    private IEnumerator Loop()
    {
        // 첫 실행까지 랜덤 대기
        yield return new WaitForSeconds(RandomRange(firstDelayRange));

        while (true)
        {
            float fadeIn = RandomRange(fadeInRange);
            float hold = RandomRange(holdRange);
            float fadeOut = RandomRange(fadeOutRange);
            float peak = Mathf.Clamp01(RandomRange(peakIntensityRange));

            // 1) Fade In (0 -> peak)
            yield return Fade(0f, peak, fadeIn);

            // 2) Hold
            yield return Hold(peak, hold);

            // 3) Fade Out (peak -> 0)
            yield return Fade(peak, 0f, fadeOut);

            StopAndClear();

            // 다음 실행까지 랜덤 대기
            yield return new WaitForSeconds(RandomRange(intervalRange));
        }
    }

    private IEnumerator Hold(float intensity, float seconds)
    {
        float t = 0f;
        float nextFollow = 0f;

        ApplyIntensity(intensity);

        while (t < seconds)
        {
            t += Time.deltaTime;

            if (followAnchorWhileActive)
            {
                if (followUpdateInterval <= 0f)
                {
                    UpdatePosition();
                }
                else
                {
                    nextFollow -= Time.deltaTime;
                    if (nextFollow <= 0f)
                    {
                        UpdatePosition();
                        nextFollow = followUpdateInterval;
                    }
                }
            }

            yield return null;
        }
    }

    private IEnumerator Fade(float from, float to, float seconds)
    {
        if (seconds <= 0f)
        {
            ApplyIntensity(to);
            yield break;
        }

        float t = 0f;
        float nextFollow = 0f;

        // 시작할 때 위치 맞춤
        UpdatePosition();

        // 켜야 보임
        if (!ps.isPlaying) ps.Play(true);

        while (t < seconds)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / seconds);
            float v = Mathf.Lerp(from, to, k);

            ApplyIntensity(v);

            if (followAnchorWhileActive)
            {
                if (followUpdateInterval <= 0f)
                {
                    UpdatePosition();
                }
                else
                {
                    nextFollow -= Time.deltaTime;
                    if (nextFollow <= 0f)
                    {
                        UpdatePosition();
                        nextFollow = followUpdateInterval;
                    }
                }
            }

            yield return null;
        }

        ApplyIntensity(to);
    }

    private void ApplyIntensity(float intensity)
    {
        intensity = Mathf.Clamp01(intensity);

        // 위치 업데이트(Anchor 없으면 이 오브젝트 위치)
        UpdatePosition();

        // Emission: Rate over Time 스케일
        var rot = baseRateOverTime;
        rot.constant *= intensity;
        rot.constantMin *= intensity;
        rot.constantMax *= intensity;
        rot.curveMultiplier *= intensity;
        emission.rateOverTime = rot;

        // Emission: Rate over Distance(선택)
        if (scaleRateOverDistance)
        {
            var rod = baseRateOverDistance;
            rod.constant *= intensity;
            rod.constantMin *= intensity;
            rod.constantMax *= intensity;
            rod.curveMultiplier *= intensity;
            emission.rateOverDistance = rod;
        }
        else
        {
            emission.rateOverDistance = 0f; // 이동/재배치 시 폭주 방지용
        }

        // Start Color 알파 컨트롤(가능할 때만)
        if (hasConstantStartColor)
        {
            Color c = baseStartColor;
            c.a = baseStartColor.a * intensity;
            main.startColor = c;
        }

        // Material 알파도 컨트롤(가능한 경우)
        if (psRenderer != null && psRenderer.sharedMaterial != null)
        {
            psRenderer.GetPropertyBlock(mpb);

            if (psRenderer.sharedMaterial.HasProperty(colorId))
            {
                var mc = psRenderer.sharedMaterial.GetColor(colorId);
                mc.a = Mathf.Clamp01(intensity);
                mpb.SetColor(colorId, mc);
            }

            if (psRenderer.sharedMaterial.HasProperty(tintColorId))
            {
                var tc = psRenderer.sharedMaterial.GetColor(tintColorId);
                tc.a = Mathf.Clamp01(intensity);
                mpb.SetColor(tintColorId, tc);
            }

            psRenderer.SetPropertyBlock(mpb);
        }

        // 완전히 꺼졌으면 Stop+Clear (잔상 제거 + 성능)
        if (intensity <= 0.001f)
        {
            StopAndClear();
        }
        else
        {
            if (!ps.isPlaying) ps.Play(true);
        }
    }

    private void StopAndClear()
    {
        if (ps == null) return;

        // 방출 중지 + 남은 파티클 제거
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
    }

    private void UpdatePosition()
    {
        if (fogInstance == null) return;

        Transform anchor = spawnAnchor != null ? spawnAnchor : transform;
        fogInstance.transform.position = anchor.position + offset;
    }

    private float RandomRange(Vector2 range)
    {
        // x <= y가 아닐 때를 방어
        float a = Mathf.Min(range.x, range.y);
        float b = Mathf.Max(range.x, range.y);
        return Random.Range(a, b);
    }
}



