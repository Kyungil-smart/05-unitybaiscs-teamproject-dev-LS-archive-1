using UnityEngine;

public class PlayerAnimation : MonoBehaviour
{
    Animator anim;
    PlayerManager player;

    void Awake()
    {
        anim = GetComponent<Animator>();
        player = GetComponent<PlayerManager>();
    }

    void Update()
    {
        anim.SetFloat("Move", player.GetMoveValue());
    }
}
