using UnityEngine;
using System.Collections;

public class DissolveController : MonoBehaviour
{
 
    [SerializeField] private Renderer _renderer;

    [SerializeField] private float _dissolveDuration = 1.5f;

    [SerializeField] private Coroutine _CurrentCrrent;

    [SerializeField] private bool _canOpenDoor=false;

    private Material mat;
    private void Awake()
    {
        _renderer=GetComponent<Renderer>();
         mat = _renderer.material;
       
    }

    private void OnTriggerEnter(Collider other)
    {
        bool isPlayer = other.gameObject.CompareTag("Player");
        if(isPlayer && _canOpenDoor)
        {
            Dissolve();
        }
    }

   


    private void Dissolve()
    {
        if (_CurrentCrrent != null) return;
         _CurrentCrrent=StartCoroutine(DissolveRoutine());
    }

   private IEnumerator DissolveRoutine()
   {
        float t = 0f;

        //일정시간 자연스럽게 Clip처리하다가
        while (t < 1f)
        {
            t += Time.deltaTime / _dissolveDuration;
            mat.SetFloat("_DissolveAmount", t);
            yield return null;
        }

        if(t>=1f)
        {
           //여기 진입 한거면 디졸브로 오브젝트 이미지 안보임
           //그러니깐 게임 오브젝트 비활성화 처리하면 될거 같음
           this.gameObject.SetActive(false);
        }
      
   }



    public void CanOpenDoor() => _canOpenDoor = true;

}
