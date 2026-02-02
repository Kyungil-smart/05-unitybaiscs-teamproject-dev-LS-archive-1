using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.ProBuilder.Shapes;

public class SkyboxHeightTransition : MonoBehaviour
{
    [SerializeField] private Cubemap _defaultCubeA;
    //[SerializeField] private Cubemap _defaultCubeB;


    [SerializeField]private List<SkyBoxData> skyDatas = new List<SkyBoxData>();
    [SerializeField] private Transform player;
    private Dictionary<int , Cubemap> skydic=new Dictionary<int , Cubemap>();
    private Cubemap CurrentSkybox;

    Material sky;
    int index = 0;
    float time = 0;
     private Coroutine CurrentCoroutine;
    private int p_idx;
    [SerializeField] private int N;
    void Start()
    {
        skydic = skyDatas.ToDictionary(x => x.idx, x => x.nextskybox);
        sky = RenderSettings.skybox;
        sky.SetTexture("_CubeA", _defaultCubeA);
        sky.SetFloat("_Blend",0f);
        index = - 1;
    }

    // Update is called once per frame
    void Update()
    {
         p_idx = (int)player.transform.position.y / N;
        if (p_idx != index )
        {
            index=p_idx;
            if (CurrentCoroutine ==null)
            {
                if (CurrentSkybox == skydic[p_idx]) return;
                CurrentCoroutine = StartCoroutine(OnChangeSkyBox());
            }
        }
    }
    private IEnumerator OnChangeSkyBox()
    {
        if(skydic.TryGetValue(p_idx, out var skybox))
        {
            CurrentSkybox = skybox;
        }
     
        sky.SetTexture("_CubeB", CurrentSkybox);
        //트랜지션 한다.
        while (time <= 1f)
        {
            time += Time.deltaTime;
            sky.SetFloat("_Blend", Mathf.Clamp01(time));
            yield return new WaitForSeconds(0.1f);
        }
        yield return StartCoroutine(OnChangeSkyBox2());
        CurrentCoroutine = null;
        Debug.Log(" 종료");
    }
    private IEnumerator OnChangeSkyBox2()
    {
        Texture current = sky.GetTexture("_CubeB");
        sky.SetTexture("_CubeA", current);
        sky.SetFloat("_Blend", 0f);
        time = 0;

        yield return null;
    }
}
