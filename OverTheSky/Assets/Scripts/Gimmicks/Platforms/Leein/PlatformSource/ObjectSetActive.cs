using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectSetActive : MonoBehaviour
{
     public GameObject myObject;
    WaitForSeconds seconds;
    public void ActiveSelf(float time)
    {
        StartCoroutine(DelayActive(time));
    }

    private IEnumerator DelayActive(float time)
    {
        if(seconds ==null)
        {
            seconds=new WaitForSeconds(time);
        }
        yield return seconds;
        myObject.SetActive(true);
    }
}
