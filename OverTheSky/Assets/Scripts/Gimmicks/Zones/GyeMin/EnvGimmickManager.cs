using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnvGimmickManager : MonoBehaviour
{
    [Header("기믹 연결")]
    [SerializeField] private FogRandomLoop fog;
    [SerializeField] private RainRandomLoop rain;
    [SerializeField] private ShakingRandomLoop shaking;

    [Header("간격(초)")]
    [SerializeField] private Vector2 firstDelayRange = new Vector2(10f, 20f);
    [SerializeField] private Vector2 intervalRange   = new Vector2(60f, 120f); // 1~2분

    [Header("셔플백 옵션")]
    [SerializeField] private bool avoidSameAcrossRounds = true;

    private readonly List<int> bag = new List<int>(3);
    private int lastIndex = -1;
    private Coroutine co;

    private void OnEnable()
    {
        if (co == null) co = StartCoroutine(Loop());
    }

    private void OnDisable()
    {
        if (co != null) { StopCoroutine(co); co = null; }
        StopAll();
    }

    private IEnumerator Loop()
    {
        yield return new WaitForSeconds(RandomRange(firstDelayRange));

        while (true)
        {
            int idx = Draw();

            StopAll();                // 시작 전 정리
            yield return Play(idx);   // 1개 실행(끝날 때까지 대기)

            StopAll();               // 추가: 끝난 직후도 정리(중첩 방지 핵심)

            yield return new WaitForSeconds(RandomRange(intervalRange));
        }
    }


    private void StopAll()
    {
        if (fog != null) fog.ForceStop();
        if (rain != null) rain.ForceStop();
        if (shaking != null) shaking.StopLoop();
    }

    private IEnumerator Play(int idx)
    {
        switch (idx)
        {
            case 0: if (fog != null)     yield return StartCoroutine(fog.PlayOnce()); break;
            case 1: if (rain != null)    yield return StartCoroutine(rain.PlayOnce()); break;
            case 2: if (shaking != null) yield return StartCoroutine(shaking.PlayOnce()); break;
        }
    }

    private int Draw()
    {
        if (bag.Count == 0)
        {
            bag.Add(0); bag.Add(1); bag.Add(2);
            Shuffle(bag);               // 한 라운드에 3개가 한 번씩 나오게(체감 랜덤 개선)

            if (avoidSameAcrossRounds && bag.Count > 1 && bag[0] == lastIndex)
            {
                (bag[0], bag[1]) = (bag[1], bag[0]);
            }
        }

        int pick = bag[0];
        bag.RemoveAt(0);
        lastIndex = pick;
        return pick;
    }

    private static void Shuffle(List<int> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }

    private static float RandomRange(Vector2 range)
    {
        float a = Mathf.Min(range.x, range.y);
        float b = Mathf.Max(range.x, range.y);
        return Random.Range(a, b);
    }
}
