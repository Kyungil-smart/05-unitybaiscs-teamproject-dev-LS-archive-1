using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OverTheSky.Core;

public class LastCheckpoint : MonoBehaviour
{
    private bool loaded = false;
    private void OnTriggerEnter(Collider other)
    {
        loaded = true;
        if (other.CompareTag("Player") && loaded)
        {
            Invoke("SceneTitleLoad", 3f);
        }
    }

    void SceneTitleLoad()
    {
        SceneController.Instance.LoadScene(Define.Scene.Title);
        loaded = false;
    }
}
