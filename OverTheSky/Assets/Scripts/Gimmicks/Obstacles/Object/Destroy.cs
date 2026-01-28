using System.Collections;
using System.Collections.Generic;
using OverTheSky.Gimmicks.Obstacles;
using UnityEngine;

namespace OverTheSky.Gimmicks.Obstacles.Object
{
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
