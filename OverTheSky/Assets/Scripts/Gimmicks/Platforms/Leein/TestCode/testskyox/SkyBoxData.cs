using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(menuName = "SkyData",fileName = "SkyBox_")]
public class SkyBoxData : ScriptableObject
{
    [SerializeField] private Cubemap a;
    [SerializeField] private int index;

    public int idx => index;
    public Cubemap nextskybox => a;
}
