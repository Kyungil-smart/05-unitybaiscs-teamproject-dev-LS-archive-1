using System;
using System.Collections;
using System.Collections.Generic;
using OverTheSky.Core;
using UnityEngine;

namespace OverTheSky.Gimmicks.Obstacles.Object
{
    public class Vehicle : MonoBehaviour
    {
        private float _moveSpeed;
        private float _pushForce = 100f;
        private float _upForce = 10f;

        private void Start()
        {
            if (gameObject.name.Contains("YellowCar"))
            {
                _moveSpeed = 50f;
            }

            if (gameObject.name.Contains("BlueCar"))
            {
                _moveSpeed = 100f;
            }
        }
        
        private void Update()
        {
            transform.Translate(Vector3.forward * _moveSpeed * Time.deltaTime);
        }

        // 플레이어 감지해서 닿으면 힘줘서 날라가는 기능
        private void OnCollisionEnter(Collision other)
        {
            if (other.gameObject.CompareTag("Player"))
            {
                // 리지드바디 대신 ForceReceiver를 가져옴.
                ForceReceiver receiver = other.gameObject.GetComponent<ForceReceiver>();

                if (receiver != null)
                {
                    // freezeRotation은 PlayerController가 이미 관리하므로 건드리지 말기.
                    // player.freezeRotation = true; 
                    
                    // 힘 벡터 합치기 (앞으로 미는 힘 + 위로 띄우는 힘)
                    Vector3 knockbackForce = (transform.forward * _pushForce) + (Vector3.up * _upForce);
                    
                    // AddImpact 호출
                    receiver.AddImpact(knockbackForce);
                }
                /*
                Rigidbody player = other.gameObject.GetComponent<Rigidbody>();

                if (player != null)
                {
                    player.freezeRotation = true;
                    
                    player.AddForce(transform.forward * _pushForce, ForceMode.VelocityChange);
                    player.AddForce(Vector3.up * _upForce, ForceMode.VelocityChange);
                }
                */
                
            }
        }
    }
}
