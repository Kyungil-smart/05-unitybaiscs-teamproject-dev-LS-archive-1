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

        
        public void ShowMessage()
        {
            _messageText.text = "Respawn : (R)";
        }
    }
}
