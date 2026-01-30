using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace OverTheSky.UI
{
    public class TimeUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _timeText;
        private float _time = 0f;
        
        public void UpdateTime()
        {
            _time += Time.deltaTime;
            TimeCal();
        }
        
        public void TimeCal()
        {
            if (_timeText != null)
            {
                int minutes = Mathf.FloorToInt(_time / 60);
                int secodns = Mathf.FloorToInt(_time % 60);
                _timeText.text = $"{minutes}:{secodns}";
            }
        }
    }
}
