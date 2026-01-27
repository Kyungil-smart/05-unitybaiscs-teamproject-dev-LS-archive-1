using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class DestroyPlatformsBase : ScriptableObject
{
    /*
      코루틴 용으로
     
 =============================================   
    BreakablePlatform(기본)
    
    밟으면 1.5초 후 부서짐
    경고 피드백 (색 변경, 흔들림)
    3초 후 재생성
 =============================================
    QuickBreakPlatform (즉시)

    밟는 순간 즉시 부서짐
    2초 후 재생성
 =============================================
    FallingObject (낙하 오브젝트)

   일정 주기로 위에서 낙하
   플레이어 충돌 시 넉백
   땅 닿으면 파괴 후 재생성
 =============================================
    1. 일정 주기로 하는건지 아닌지로 분기 나뉜다.


*/
    //밟았을 때 N초 후 발판 사라지는 타이머 
    [SerializeField] private float m_Time = 0f;
    public float Time => m_Time;

    //코루틴이 반복하는지 안하는지
    [SerializeField] private bool m_repeat = false;
    public bool repeat => m_repeat;

    //비활성화 된 후 몇 초뒤에 다시 활성화 될것인지
    [SerializeField] private float m_respawnTime = 0f;
    public float respawnTime => m_respawnTime;

    //플레이어가 발판 밟았을 때 N초 뒤에 게임오브젝트 비활성
    public abstract void OnGimmic();

    public abstract void Init(DestroyPlatforms Owner,GameObject invisible_Object);
    //

 
    public abstract IEnumerator WarningColorRoutine(Renderer render);
}
