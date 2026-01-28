using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
[CreateAssetMenu(fileName = "BreakablePlatform_", menuName = "DestroyGimmic/BreakablePlatform", order = 1)]
public class BreakablePlatform : DestroyPlatformsBase
{
    private DestroyPlatforms m_owner;
    private GameObject m_invisible;
    ObjectSetActive _active;
    #region
    [SerializeField] private Renderer targetRenderer;

    
    [SerializeField] private Color startColor = Color.white;
    [SerializeField] private Color targetColor = Color.red;
    #endregion
    public override void Init(DestroyPlatforms Owner, GameObject invisible_Object, ObjectSetActive _active)
    {
        m_owner=Owner;
        m_invisible=invisible_Object;
        this._active = _active;
    }

    public override void OnGimmic()
    {
        Debug.Log("종료");
        _active?.ActiveSelf(respawnTime);
        m_invisible.SetActive(false);
    }

    public override IEnumerator RunForSeconds(Renderer render)
    {
        float elapsed = 0f;
        Material mat = render.material;

        while (elapsed < Time)
        {
            elapsed += UnityEngine.Time.deltaTime;

            float t = Mathf.Clamp01(elapsed / Time);
            mat.color = Color.Lerp(startColor, targetColor, t);

            yield return null;
        }

        // 오차 방지 (정확히 목표 색으로 고정)
        mat.color = targetColor;
        Debug.Log("exit");
    }


}
