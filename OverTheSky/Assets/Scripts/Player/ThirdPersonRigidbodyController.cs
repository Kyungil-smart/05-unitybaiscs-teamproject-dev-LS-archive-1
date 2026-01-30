using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
public class ThirdPersonRigidbodyController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform visualRoot;
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private Animator animator;

    [Header("Layers")]
    [SerializeField] private LayerMask groundLayer;

    [Header("Move")]
    [SerializeField] private float walkSpeed = 4f;
    [SerializeField] private float runSpeed = 6.5f;
    [SerializeField] private float acceleration = 35f;
    [SerializeField] private float rotationSpeed = 12f;

    [Header("Jump & Gravity (Use Gravity OFF)")]
    [SerializeField] private float gravity = 25f;
    [SerializeField] private float jumpHeight = 1.2f;
    [SerializeField] private float fallMultiplier = 1.8f;
    [SerializeField] private float groundStickForce = 25f;

    [Tooltip("점프 버튼을 길게 누르면 더 높게, 짧게 누르면 낮게")]
    [SerializeField] private float jumpCutMultiplier = 2.2f;

    [Header("Jump Assist (Quality)")]
    [Tooltip("바닥에서 살짝 떨어진 뒤에도 점프 허용(초)")]
    [SerializeField] private float coyoteTime = 0.12f;

    [Tooltip("점프 입력을 미리 눌러도 착지 순간 점프(초)")]
    [SerializeField] private float jumpBufferTime = 0.12f;

    [Header("Drag (Dynamic Control)")]
    [SerializeField] private float groundDrag = 15f;
    [SerializeField] private float airDrag = 0.5f;

    [Header("Ground Detection (SphereCast)")]
    [SerializeField] private float groundCheckDistance = 0.2f;
    [SerializeField] private float groundedNormalMinY = 0.7f;
    [SerializeField] private float maxSlopeAngle = 45f;

    [Header("Ceiling Check")]
    [SerializeField] private float ceilingCheckDistance = 0.15f;

    private static readonly int AnimSpeed = Animator.StringToHash("Speed");
    private static readonly int AnimJump = Animator.StringToHash("Jump");
    private static readonly int AnimGrounded = Animator.StringToHash("Grounded");
    private static readonly int AnimFreeFall = Animator.StringToHash("FreeFall");
    private static readonly int AnimMotionSpeed = Animator.StringToHash("MotionSpeed");

    private Rigidbody rb;
    private CapsuleCollider capsule;

    private Vector2 moveInput;
    private bool runHeld;

    private float jumpBufferCounter;
    private float coyoteCounter;
    private bool jumpHeld;

    private bool isGrounded;
    private bool wasGrounded;
    private bool isJumping;
    private Vector3 groundNormal = Vector3.up;

    private Rigidbody currentPlatformRb;
    private Vector3 platformVelocity;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        capsule = GetComponent<CapsuleCollider>();
        rb.useGravity = false;
    }

    private void Update()
    {
        moveInput.x = Input.GetAxisRaw("Horizontal");
        moveInput.y = Input.GetAxisRaw("Vertical");
        moveInput = Vector2.ClampMagnitude(moveInput, 1f);

        runHeld = Input.GetKey(KeyCode.LeftShift);

        if (Input.GetKeyDown(KeyCode.Space))
            jumpBufferCounter = jumpBufferTime;

        jumpHeld = Input.GetKey(KeyCode.Space);
    }

    private void FixedUpdate()
    {
        wasGrounded = isGrounded;

        GroundCheck();
        ApplyDynamicDrag();
        UpdatePlatformVelocity();

        if (isGrounded) coyoteCounter = coyoteTime;
        else coyoteCounter -= Time.fixedDeltaTime;

        jumpBufferCounter -= Time.fixedDeltaTime;

        HandleMovement();
        HandleJumpAndGravity();
        HandleCeiling();
        UpdateAnimator();
    }

    private void GroundCheck()
    {
        isGrounded = false;
        groundNormal = Vector3.up;
        currentPlatformRb = null;

        Vector3 lossy = transform.lossyScale;
        float radiusScale = Mathf.Max(Mathf.Abs(lossy.x), Mathf.Abs(lossy.z));
        float heightScale = Mathf.Abs(lossy.y);

        float sphereRadius = capsule.radius * radiusScale * 0.9f;
        float halfHeight = capsule.height * 0.5f * heightScale;

        Vector3 center = rb.position + (transform.rotation * Vector3.Scale(capsule.center, lossy));
        float castDist = Mathf.Max(0f, halfHeight - sphereRadius) + groundCheckDistance;

        if (Physics.SphereCast(center, sphereRadius, Vector3.down, out RaycastHit hit, castDist, groundLayer, QueryTriggerInteraction.Ignore))
        {
            if (hit.normal.y > groundedNormalMinY)
            {
                isGrounded = true;
                groundNormal = hit.normal;
                currentPlatformRb = hit.rigidbody;

                if (!wasGrounded && isGrounded)
                    isJumping = false;
            }
        }

        if (wasGrounded && !isGrounded && !isJumping)
        {
            if (animator != null)
                animator.SetTrigger(AnimFreeFall);
        }
    }

    private void ApplyDynamicDrag()
    {
        rb.drag = isGrounded ? groundDrag : airDrag;
        rb.angularDrag = 0f;
    }

    private void UpdatePlatformVelocity()
    {
        platformVelocity = Vector3.zero;
        if (isGrounded && currentPlatformRb != null)
            platformVelocity = currentPlatformRb.GetPointVelocity(rb.position);
    }

    private void HandleMovement()
    {
        Vector3 camForward = Vector3.forward;
        Vector3 camRight = Vector3.right;

        if (cameraTransform != null)
        {
            camForward = cameraTransform.forward;
            camRight = cameraTransform.right;
            camForward.y = 0f;
            camRight.y = 0f;
            camForward.Normalize();
            camRight.Normalize();
        }

        Vector3 inputDir = (camForward * moveInput.y + camRight * moveInput.x);
        inputDir = Vector3.ClampMagnitude(inputDir, 1f);

        Vector3 moveDir = inputDir;

        if (isGrounded && moveDir.sqrMagnitude > 0.0001f)
            moveDir = Vector3.ProjectOnPlane(moveDir, groundNormal).normalized;

        if (isGrounded)
        {
            float slopeAngle = Vector3.Angle(groundNormal, Vector3.up);
            if (slopeAngle > maxSlopeAngle)
                moveDir = Vector3.zero;
        }

        float targetSpeed = runHeld ? runSpeed : walkSpeed;
        Vector3 targetHorizontalVel = moveDir * targetSpeed;

        Vector3 currentVel = rb.velocity;
        Vector3 relVel = currentVel - platformVelocity;

        Vector3 currentHorizontal = new Vector3(relVel.x, 0f, relVel.z);

        Vector3 newHorizontal = Vector3.MoveTowards(
            currentHorizontal,
            targetHorizontalVel,
            acceleration * Time.fixedDeltaTime
        );

        rb.velocity = new Vector3(newHorizontal.x, relVel.y, newHorizontal.z) + platformVelocity;

        if (visualRoot != null && moveDir.sqrMagnitude > 0.0001f)
        {
            Quaternion targetRot = Quaternion.LookRotation(moveDir, Vector3.up);
            visualRoot.rotation = Quaternion.Slerp(visualRoot.rotation, targetRot, rotationSpeed * Time.fixedDeltaTime);
        }
    }

    private void HandleJumpAndGravity()
    {
        Vector3 v = rb.velocity;

        bool canJump = (jumpBufferCounter > 0f) && (isGrounded || coyoteCounter > 0f);

        if (canJump)
        {
            jumpBufferCounter = 0f;
            coyoteCounter = 0f;

            isJumping = true;
            isGrounded = false;

            float jumpVelocity = Mathf.Sqrt(2f * gravity * Mathf.Max(0.01f, jumpHeight));
            v.y = jumpVelocity;
            rb.velocity = v;

            if (animator != null)
                animator.SetTrigger(AnimJump);

            return;
        }

        if (!isGrounded)
        {
            float g = gravity;

            if (v.y < 0f) g *= fallMultiplier;
            if (!jumpHeld && v.y > 0f) g *= jumpCutMultiplier;

            v.y -= g * Time.fixedDeltaTime;
            rb.velocity = v;
        }
        else
        {
            if (!isJumping)
            {
                v.y -= groundStickForce * Time.fixedDeltaTime;
                rb.velocity = v;
            }
        }
    }

    private void HandleCeiling()
    {
        if (rb.velocity.y <= 0f) return;

        Vector3 lossy = transform.lossyScale;
        float radiusScale = Mathf.Max(Mathf.Abs(lossy.x), Mathf.Abs(lossy.z));
        float heightScale = Mathf.Abs(lossy.y);

        float sphereRadius = capsule.radius * radiusScale * 0.85f;
        float halfHeight = capsule.height * 0.5f * heightScale;

        Vector3 center = rb.position + (transform.rotation * Vector3.Scale(capsule.center, lossy));
        Vector3 origin = center + Vector3.up * Mathf.Max(0f, halfHeight - sphereRadius);

        if (Physics.SphereCast(origin, sphereRadius, Vector3.up, out RaycastHit hit, ceilingCheckDistance, groundLayer, QueryTriggerInteraction.Ignore))
        {
            if (hit.normal.y < 0f)
            {
                Vector3 v = rb.velocity;
                v.y = 0f;
                rb.velocity = v;
            }
        }
    }

    private void UpdateAnimator()
    {
        if (animator == null) return;

        animator.SetBool(AnimGrounded, isGrounded);

        Vector3 horizontal = rb.velocity;
        horizontal.y = 0f;

        float max = Mathf.Max(0.01f, runSpeed);
        float speed01 = Mathf.Clamp01(horizontal.magnitude / max);

        animator.SetFloat(AnimSpeed, speed01);
        animator.SetFloat(AnimMotionSpeed, 1f);
    }
}



