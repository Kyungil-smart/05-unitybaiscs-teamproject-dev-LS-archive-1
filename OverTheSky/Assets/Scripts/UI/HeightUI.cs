using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace OverTheSky.UI
{
    public class HeightUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _heightUI;

        // 높이값을 표시하는 부분
        public void UpdateHeight(int height)
        {
            _heightUI.text = $"{height} m";
        }
    }
    
}
