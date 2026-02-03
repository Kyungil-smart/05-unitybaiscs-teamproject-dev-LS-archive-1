using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Surface : MonoBehaviour
{
    [SerializeField]private List<SurfaceData> _surfaces=new List<SurfaceData>();
    private Dictionary<LayerMask, SurfaceData> _surfacesDictionary=new Dictionary<LayerMask, SurfaceData>();

    [SerializeField] private SurfaceData CurrentSurface;

    public float accel => CurrentSurface.accel;
    public float decel => CurrentSurface.decel;

    void Start()
    {
        _surfacesDictionary = _surfaces.ToDictionary(x => x.layerMask, x => x);
        CurrentSurface = _surfaces[0];
    }

   public void OnChangeData(LayerMask m_Layer)
   {
       CurrentSurface = _surfacesDictionary[m_Layer];
   }

    public void OnExitData()
    {
        CurrentSurface = _surfaces[0];
    }
}
