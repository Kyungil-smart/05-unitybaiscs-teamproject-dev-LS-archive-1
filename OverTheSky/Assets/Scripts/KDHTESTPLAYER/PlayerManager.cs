using System.Collections;
using System.Collections.Generic;
using OverTheSky.Core;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    IMove move;
    IRotate rotate;

    float moveValue;
    private bool _isGrounded;
    private Rigidbody _rb;
    [SerializeField] private float _jumpForce = 5f;
    void Start()
    {
        move = GetComponent<IMove>();
        rotate = GetComponent<IRotate>();      
        _rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        float h = Input.GetAxisRaw("Horizontal");       // A D
        float v = Input.GetAxisRaw("Vertical");         // W S
        Vector2 input = new Vector2(h, v);
        UIManager.Instance.UpdateUI(transform.position.y);
        moveValue = move?.Invoke(input) ?? 0;
        rotate?.Invoke(h);
        if (Input.GetKeyDown(KeyCode.Space) && _isGrounded)
        {
            _rb.AddForce(Vector3.up * _jumpForce, ForceMode.Impulse);
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        _isGrounded = true;
    }

    private void OnCollisionExit(Collision collision)
    {
        _isGrounded = false;
    }
    public float GetMoveValue() => moveValue;
}
