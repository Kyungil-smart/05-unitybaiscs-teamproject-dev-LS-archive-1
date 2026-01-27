using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OverTheSky.Systems
{
    public class CheckPoint : MonoBehaviour
    {
        [SerializeField] private GameObject _checkPoint;

        // Player태그를 가진 유저가 닿으면 등록
        private void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.CompareTag("Player"))
            {
                ChangeCheckPoint();
            }
        }
        
        // 체크포인트 등록하는 부분
        public void ChangeCheckPoint()
        {
            Instantiate(_checkPoint, transform.position, transform.rotation);
            Destroy(gameObject);
        }
    }
}
