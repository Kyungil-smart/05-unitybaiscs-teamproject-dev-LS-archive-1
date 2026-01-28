using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OverTheSky.Gimmicks.Obstacles.Object
{
    public class StartTrigger : MonoBehaviour
    {
        [SerializeField] private OverTheSky.Gimmicks.Obstacles.Vehicle_Spawn _vehicleSpawn;
        private Coroutine _vehicleSpawnCoroutine1;
        private Coroutine _vehicleSpawnCoroutine2;
        
        // 트리거 진입시 코루틴 시작
        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                _vehicleSpawnCoroutine1 = StartCoroutine(_vehicleSpawn._vehicleSpawnCoroutine1());
                _vehicleSpawnCoroutine2 = StartCoroutine(_vehicleSpawn._vehicleSpawnCoroutine2());
            }
        }

        // 트리거 나가면 코루틴 종료
        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                StopCoroutine(_vehicleSpawnCoroutine1);
                StopCoroutine(_vehicleSpawnCoroutine2);
                _vehicleSpawnCoroutine1 = null;
                _vehicleSpawnCoroutine2 = null;
            }
        }
    }
}
