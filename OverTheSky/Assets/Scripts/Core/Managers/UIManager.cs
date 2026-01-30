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
        
        protected override void Awake()
        {
            base.Awake();
        }

        // Player에 y값을 받아와 HeightUI에 값을 넘기는 부분
        public void UpdateHeight(float height)
        {
            int _height = (int)height;
            _heightUI.UpdateHeight(_height);
        }
    }
}
