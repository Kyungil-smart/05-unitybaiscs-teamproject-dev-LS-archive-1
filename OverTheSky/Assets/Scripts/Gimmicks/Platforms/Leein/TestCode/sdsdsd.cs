using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class sdsdsd : MonoBehaviour
{
    [SerializeField] private Rigidbody rigid;
    void Start()
    {
        rigid.AddForce(-this.transform.forward * 20f,ForceMode.Impulse);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
