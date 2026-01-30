using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "NewSurfaceData",fileName = "SurfaceData_")]
public class SurfaceData : ScriptableObject
{
    [SerializeField] private LayerMask m_layer;
    public LayerMask layerMask=> m_layer;

    //가속
    [SerializeField] private float m_accel;
    public float accel => m_accel;

    //감속
    [SerializeField] private float m_decel;
    public float decel => m_decel; 
}
