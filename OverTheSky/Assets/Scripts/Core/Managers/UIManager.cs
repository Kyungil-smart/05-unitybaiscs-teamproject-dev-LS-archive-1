using System;
using System.Collections;
using System.Collections.Generic;
using OverTheSky.Core;
using OverTheSky.UI;
using TMPro;
using UnityEngine;

namespace OverTheSky.Core
{
    public class UIManager : Singleton<UIManager>
    {
        [SerializeField] private TimeUI _timeUI;
        [SerializeField] private MessageUI _messageUI;
        [SerializeField] private HeightUI _heightUI;
        [SerializeField] private Transform _player;
        
        protected override void Awake()
        {
            base.Awake();
            if (_player == null)
            {
                GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
                if (playerObj != null)
                {
                    _player = playerObj.transform;
                }
            }
            _messageUI.ShowMessage();
        }

        private void Update()
        {
            int height = (int)_player.position.y;
            _heightUI.UpdateHeight(height);
            _timeUI.UpdateTime();
        }

        // Player에 y값을 받아와 HeightUI에 값을 넘기는 부분
        public void UpdateHeight(float height)
        {
            int _height = (int)height;
            _heightUI.UpdateHeight(_height);
        }
        
        public void StartTimer()
        {
            if (_timeUI != null)
            {
                _timeUI.TimeCal();
            }
        }
        
        public void ShowMessage()
        {
            if (_messageUI != null)
            {
                _messageUI.ShowMessage();
            }
        }
    }
}
