using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerKnockback : MonoBehaviour
{
    [SerializeField] private Rigidbody _rigid;
    [SerializeField] private Vector3 _direction;
    [SerializeField] private float _Power;
    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.CompareTag("Player"))
        {
            _rigid= other.GetComponent<Rigidbody>();
            _direction= -_rigid.transform.forward;
            _rigid.AddForce(_direction* _Power,ForceMode.Impulse);
        }
    }
}
