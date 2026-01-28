using System.Collections;
using System.Collections.Generic;
using OverTheSky.Systems;
using UnityEngine;

namespace OverTheSky.Core
{
    public class CheckpointManager : MonoBehaviour
    {
        private static CheckpointManager _instance;
        public static CheckpointManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<CheckpointManager>();
                }
                return _instance;
            }
        }
        
        private Vector3 _lastCheckpointPosition;
        private Quaternion _lastCheckpointRotation;

        // 마지막 체크포인트 위치 등록
        public void RegisterCheckpoint(Vector3 pos, Quaternion rot)
        {
            _lastCheckpointPosition = pos;
            _lastCheckpointRotation = rot;
        }

        // 마지막 체크포인트 위치를 가져옴
        public void GetSpawnPoints(out Vector3 position, out Quaternion rotation)
        {
            position = _lastCheckpointPosition;
            rotation = _lastCheckpointRotation;
        }
    }
}
