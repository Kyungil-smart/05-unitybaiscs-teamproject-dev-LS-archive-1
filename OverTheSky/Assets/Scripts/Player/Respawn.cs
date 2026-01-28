using System;
using System.Collections;
using System.Collections.Generic;
using OverTheSky.Core;
using UnityEngine;

public class Respawn : MonoBehaviour
{
    // 플레이어 처음위치를 첫 체크포인트로 지정
    private void Start()
    {
        CheckpointManager.Instance.RegisterCheckpoint(transform.position, transform.rotation);
    }

    // R키를 누르면 마지막 체크포인트에서 리스폰
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R)) PlayerRespawn();
    }

    // 체크포인트로 이동하는 기능 RigidBody의 속도 초기화
    public void PlayerRespawn()
    {
        Vector3 playerPos;
        Quaternion playerRot;
        
        CheckpointManager.Instance.GetSpawnPoints(out playerPos, out playerRot);
        
        transform.position = playerPos;
        transform.rotation = playerRot;
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }
}
