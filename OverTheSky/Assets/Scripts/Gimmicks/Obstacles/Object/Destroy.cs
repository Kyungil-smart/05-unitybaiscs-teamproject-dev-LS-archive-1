using System.Collections;
using System.Collections.Generic;
using OverTheSky.Gimmicks.Obstacles;
using UnityEngine;

namespace OverTheSky.Gimmicks.Obstacles.Object
{
    
    // 차가 맵(도로)밖으로 나가면 사라지는 기능
    public class Destroy : MonoBehaviour
    {
        private void OnTriggerEnter(Collider other)
        {
            if (other.GetComponent<Vehicle>() != null)
            {
                Destroy(other.gameObject);
            }
        }
    }
}
