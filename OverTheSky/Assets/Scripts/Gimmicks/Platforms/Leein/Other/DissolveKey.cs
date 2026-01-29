using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DissolveKey : MonoBehaviour
{
    [SerializeField] private DissolveController _dissolveController;

     

    private  delegate void MyAction();
    private MyAction _triggerAction; 

    private void Awake()
    {
        _triggerAction = _dissolveController.CanOpenDoor;
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.CompareTag("Player"))
        {
            _triggerAction?.Invoke();
            this.gameObject.SetActive(false);
        }
    }
}
