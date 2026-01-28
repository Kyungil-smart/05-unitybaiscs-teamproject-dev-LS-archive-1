using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "QuickBreakPlatform_", menuName = "DestroyGimmic/QuickBreakPlatform", order = 2)]
public class QuickBreakPlatform : DestroyPlatformsBase
{

    private DestroyPlatforms m_Owner;
    private GameObject m_invisible;
    ObjectSetActive _active;
    public override void Init(DestroyPlatforms Owner, GameObject invisible_Object, ObjectSetActive _active)
    {
        m_Owner=Owner;
        m_invisible=invisible_Object;
        this._active = _active;
    }

    public override IEnumerator RunForSeconds(Renderer render)
    {
        yield return null;
    }

    public override void OnGimmic()
    {
        _active?.ActiveSelf(respawnTime);
        m_invisible?.SetActive(false);
    }
}
