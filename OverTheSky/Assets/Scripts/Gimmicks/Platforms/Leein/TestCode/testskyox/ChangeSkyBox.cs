using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeSkyBox : MonoBehaviour
{
    [SerializeField] private Cubemap _defaultCubeA;
    [SerializeField] private Cubemap _defaultCubeB;

    [SerializeField] private Cubemap cubeA;
    [SerializeField] private Cubemap cubeB;
    Material sky;
    float time=0f;
   
    void Start()
    {
        sky = RenderSettings.skybox;
        sky.SetTexture("_CubeA", _defaultCubeA);
        sky.SetTexture("_CubeB", _defaultCubeB);
        if (cubeA == null) return;
       
        
        sky.SetTexture("_CubeA", cubeA);
        sky.SetTexture("_CubeB", cubeB);
        StartCoroutine(OnChangeSkyBox());
    }

    [ContextMenu("Test")]
    void Test()
    {
        StartCoroutine(OnChangeSkyBox());
    }
    private IEnumerator OnChangeSkyBox()
    {
        while(time<=1f)
        {
            time += Time.deltaTime ;
            sky.SetFloat("_Blend", Mathf.Clamp01(time));
            yield return new WaitForSeconds(0.1f);
        }
        Debug.Log("종료");
        Destroy(this);
    }

    [ContextMenu("Test2")]
    void Test2()
    {
        StartCoroutine(OnChangeSkyBox2());
    }
    private IEnumerator OnChangeSkyBox2()
    {
        sky = RenderSettings.skybox;
        Texture current = sky.GetTexture("_CubeB");
        sky.SetTexture("_CubeA", current);
        sky.SetTexture("_CubeB", cubeB);
        sky.SetFloat("_Blend", 0f);
        time = 0;
        yield return StartCoroutine(OnChangeSkyBox());
    }
    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.D))
        {
            StartCoroutine(OnChangeSkyBox2());
         
        }
    }
    //private void Update()
    //{
    //   if(Input.GetKey(KeyCode.A))
    //    {
    //        time += Time.deltaTime* ttime;
    //        sky.SetFloat("_Blend", Mathf.Clamp01(time));
    //    }
    //    if (Input.GetKey(KeyCode.B))
    //    {
    //        time -= Time.deltaTime* ttime;
    //        sky.SetFloat("_Blend", Mathf.Clamp01(time));
    //    }
    //    if (Input.GetKeyDown(KeyCode.D))
    //    {
    //        Texture current = sky.GetTexture("_CubeB");
    //        sky.SetTexture("_CubeA", current);
    //        sky.SetTexture("_CubeB", cubeA);
    //        sky.SetFloat("_Blend", 0f);
    //        time = 0;

    //    }
    //}
    /*
     현재 큐브박스를 A라고하고 저장된 큐브를 B
    콜라이더 나갈 때 작동 시켜서
    오브젝트 기준 플레이어가 앞에 있거나 뒤에 있거나 판별해서

    현재 코드에서 재사용하려면 Cube A,B기본적으로 초기에 다 있어야하고

    다른 오브젝트에 이 스크립트 붙일 때 CubeA를 빼고 CubeB만 넣는다.
    그렇게 해서 특정 지역 들어갈 때 트리거가 당겨지면
    현재 스카이박스와 스크립트에 저장된 스카이박스 B를 셰이더로 한번 더 보내서 저장한다.
    그리고 보간을 실행한다.

    여기서 잠깐 플레이어 고도별 스카이박스 체인지 여서 어떻게 하면 좋을까 생각중
    
    플레이어가 특정 길에서 트리거 콜라이더랑 닿아서 위치 판별 후 스카이 박스 교체 까지는 생각해봤는데
    만약 떨어진다면 어떻게 해야할것인가?


   그리고 다음 교체 때 변경된 Blend값을 0으로 셋팅해줘야하고
   skybox B를 skybox A변수에 저장해야 할거 같다  
  
     (그냥 플레이어 transform 참조해서 하는걸로 바꿈 설계 다시해야함)
     */
    ////////////////////////////////////////////////////////////////////////////////
    /*
     플레이어 Y / N 을 해서 몫을 index 번호로 해서 dicionary로 sky박스 빼온다.
     빼온 스카이 박스는 중첩안되게 임시 저장 변수에 저장을 한다.


    일단 현재 스카이 박스랑 꺼내온 박스랑 같은게 아닌지 판별 후 
    sky.SetTexture("_CubeB", cubeB); <--이거 써서 다음 스카이 박스 저장한다.
    그리고 현재 스카이박스에서 바꿀 스카이 박스를  트랜지션 한다.

    트랜지션이 완료 되면
    현재 보여지고 있는 스카이박스를 cubeA에 저장하고 sky.SetTexture("_CubeA", current); 
     blend값을 0 으로 초기화 한 한다.
   
    */

}
