using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace OverTheSky.UI
{
    public class MessageUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _messageText;

        
        private void Update()
        {
            _messageText.text = "Respawn (R)";
        }
    }
}
