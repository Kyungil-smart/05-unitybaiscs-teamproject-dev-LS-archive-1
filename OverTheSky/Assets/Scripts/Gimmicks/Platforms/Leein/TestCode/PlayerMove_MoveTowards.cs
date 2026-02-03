using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMove_MoveTowards : MonoBehaviour
{
    public float maxSpeed = 6f;
    [SerializeField] private Surface surface;
    
    Rigidbody rb;
    Vector3 inputDir;
    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
    }

    void Update()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        inputDir = new Vector3(h, 0f, v).normalized;
    }

    void FixedUpdate()
    {
        Vector3 current = rb.velocity;
        Vector3 target = inputDir * maxSpeed;
        target.y = current.y; // 중력 유지

        float rate = inputDir.magnitude > 0 ? surface.accel : surface.decel;

        rb.velocity = Vector3.MoveTowards(
            current,
            target,
            rate * Time.fixedDeltaTime
        );
    }
}
