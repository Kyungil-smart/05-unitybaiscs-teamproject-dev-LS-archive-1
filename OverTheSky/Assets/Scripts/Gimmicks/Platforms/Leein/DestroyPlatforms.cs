using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;
using UnityEngine.UIElements;
using Color = UnityEngine.Color;

public class DestroyPlatforms : MonoBehaviour
{
    [SerializeField] private DestroyPlatformsBase _platforms;
    [SerializeField] private ObjectSetActive _active;
    private Coroutine CurrentCoroutine;
    private WaitForSeconds Seconds;


    #region
    [SerializeField] private Renderer targetRenderer;
 
    #endregion
    private void Awake()
    {
        if (Seconds != null) return;
        targetRenderer = GetComponent<Renderer>();
         Seconds = new(_platforms.Time);

        //발판 초기화 :  주체가 되는 오브젝트 탐색하기위해 자신과,비활성할 게임오브젝트 초기화
        _platforms.Init(this, this.gameObject);
    }

    private void Start()
    {
        //테스트용 코드입니다.
        //if (CurrentCoroutine != null) return;
        //CurrentCoroutine = StartCoroutine(StartGimmic());
        StartCoroutine(StartGimmic());
    }

    private void OnCollisionEnter(Collision collision)
    {
        //플레이가 발판 밟았을 때 실행용
        //밟았다가 다시 밟아서 한번 더 실행되지 않게 설정 
        if (CurrentCoroutine != null) return;
         CurrentCoroutine = StartCoroutine(StartGimmic());
    }
    

    private IEnumerator StartGimmic()
    {
        while (true)
        {
            yield return StartCoroutine(_platforms.RunForSeconds(targetRenderer));

            _platforms.OnGimmic();

            if (!_platforms.repeat)
                yield break;
        }

    }

    //테스트 용 
    private void OnEnable() => StartCoroutine(StartGimmic());

    //오브젝트 비활성화 되면 기존에 설정한 시간 뒤에 오브젝트 재활성화
    private void OnDisable() => _active.ActiveSelf(_platforms.respawnTime);

}
