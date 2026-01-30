using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Change : MonoBehaviour
{
    public LayerMask LayerMask;

    private void OnTriggerStay(Collider other)
    {
        if(other.gameObject.CompareTag("Player"))
        {
            var data= other.gameObject.GetComponent<Surface>();
            data.OnChangeData(LayerMask);
        }


         
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            var data = other.gameObject.GetComponent<Surface>();
            data.OnExitData();
        }
    }
}
