using UnityEngine;

public class RotateComponent : MonoBehaviour, IRotate
{
    [SerializeField] private float _rotateSpeed = 180f;

    public void Invoke(float horizontal)
    {
        if(Mathf.Abs(horizontal) < 0.01f) return;

        float angle = horizontal * _rotateSpeed * Time.deltaTime;
        transform.Rotate(Vector3.up, angle);
    }
}
