using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OverTheSky.Gimmicks.Obstacles
{
    public class Vehicle_Spawn : MonoBehaviour
    {
        [SerializeField] private GameObject _vehiclePrefab1;
        [SerializeField] private GameObject _vehiclePrefab2;
        [SerializeField] private Transform _spawnPoint1;
        [SerializeField] private Transform _spawnPoint2;

        // 차량 오브젝트의 갯수가 적어서 각각을 직접 선언함.
        public IEnumerator _vehicleSpawnCoroutine1()
        {
            while (true)
            {
                Instantiate(_vehiclePrefab1, _spawnPoint1.position, _spawnPoint1.rotation);
                yield return new WaitForSeconds(1f);
            }
        }
        
        public IEnumerator _vehicleSpawnCoroutine2()
        {

            while (true)
            {
                Instantiate(_vehiclePrefab2, _spawnPoint2.position, _spawnPoint2.rotation);
                yield return new WaitForSeconds(1.5f);
            }
        }
    }
}
