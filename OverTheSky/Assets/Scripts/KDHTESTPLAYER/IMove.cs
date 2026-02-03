using UnityEngine;

public interface IMove
{
    public float Invoke(Vector2 input);    // W S 이동
}

public interface IRotate
{
    public void Invoke(float horizontal);  // A D 회전
}

