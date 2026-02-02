using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OverTheSky.Core;
public class PlayerKnockback : MonoBehaviour
{
 
    
    [SerializeField] private float _Power;
    [SerializeField] private Rigidbody _rigid;
    [SerializeField] private bool colision;
    [SerializeField] Vector3 power;
  
    private void OnTriggerEnter(Collider other)
    {
        if(!other.gameObject.CompareTag("Player"))
            return;
        power = _rigid.velocity;
        Rigidbody rb;
        ForceReceiver forceReceiver;
        if (!other.gameObject.TryGetComponent(out rb)) return;
        if (!other.gameObject.TryGetComponent(out forceReceiver));
        forceReceiver.AddImpact(other.transform.right* _Power);
        Debug.Log("넉백실행");
        colision = true;
    }
    private void FixedUpdate()
    {
       if( colision)
        {
            _rigid.velocity = -power; 
        }
    }
    private void OnEnable()
    {
        _rigid.velocity = Vector3.zero;
    }
    private void OnDisable()
    {
        colision=false; 
    }
}
