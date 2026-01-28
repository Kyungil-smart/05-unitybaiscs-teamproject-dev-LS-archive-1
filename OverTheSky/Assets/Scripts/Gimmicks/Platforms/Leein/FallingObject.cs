using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "FallingObject_", menuName = "DestroyGimmic/FallingObject", order = 3)]
public class FallingObject : DestroyPlatformsBase
{
    private DestroyPlatforms m_Owner;
    private GameObject _object;
    private Rigidbody _rigid;
    private WaitForSeconds seconds;
    [SerializeField] private float _AddForcePower;
    public override void Init(DestroyPlatforms Owner, GameObject invisible_Object)
    {
        m_Owner=Owner;
        _rigid= invisible_Object.GetComponent<Rigidbody>();
        seconds = new WaitForSeconds(Time);
    }

    public override IEnumerator RunForSeconds(Renderer render)
    {
        _rigid.useGravity = false;
        yield return seconds;
        _rigid.useGravity = true;
    }

    public override void OnGimmic()
    {
       
        _rigid.AddForce(Vector3.down * _AddForcePower, ForceMode.VelocityChange);
        Debug.Log($"{Time}뒤에 실행");
    }
}
