using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RunComponent : MonoBehaviour, IMove
{
    [SerializeField] private float _speed = 5f;
    [SerializeField] private float _runSpeed = 12f;

    public float Invoke(Vector2 input)
    {
        if (input.magnitude <= 0f) return 0f;

        bool isRun = Input.GetKey(KeyCode.LeftShift);
        float speed = isRun ? _runSpeed : _speed;       // 삼항연산자

        Vector3 dir = Vector3.forward * input.y;
        transform.Translate(dir * speed * Time.deltaTime);

        return isRun ? 1.0f : 0.5f;     // 애니메이션 파트..
    }
}
