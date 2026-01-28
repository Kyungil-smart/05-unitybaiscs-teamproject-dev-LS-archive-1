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

    // Animator Params (5개)
    private static readonly int AnimSpeed = Animator.StringToHash("Speed");
    private static readonly int AnimJump = Animator.StringToHash("Jump");
    private static readonly int AnimGrounded = Animator.StringToHash("Grounded");
    private static readonly int AnimFreeFall = Animator.StringToHash("FreeFall");
    private static readonly int AnimMotionSpeed = Animator.StringToHash("MotionSpeed");

    private Rigidbody rb;
    private CapsuleCollider capsule;

    private Vector2 moveInput;
    private bool runHeld;

    // 점프 입력(버퍼)
    private float jumpBufferCounter;
    // 코요테 타임(지면 떠난 직후)
    private float coyoteCounter;

    private bool jumpHeld; // 점프를 누르고 있는지(짧게/길게 점프용)

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
        rb.useGravity = false; // 코드 중력
    }

    private void Update()
    {
        moveInput.x = Input.GetAxisRaw("Horizontal");
        moveInput.y = Input.GetAxisRaw("Vertical");
        moveInput = Vector2.ClampMagnitude(moveInput, 1f);

        runHeld = Input.GetKey(KeyCode.LeftShift);

        // 점프 버퍼: 누르면 타이머 채우기
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

        // 코요테 타임 갱신
        if (isGrounded) coyoteCounter = coyoteTime;
        else coyoteCounter -= Time.fixedDeltaTime;

        // 점프 버퍼 카운트다운
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

        float sphereRadius = capsule.radius * 0.9f;
        Vector3 origin = rb.position + Vector3.up * sphereRadius;

        if (Physics.SphereCast(origin, sphereRadius, Vector3.down, out RaycastHit hit,
            groundCheckDistance, groundLayer, QueryTriggerInteraction.Ignore))
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

        // 점프 없이 떨어질 때 FreeFall
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
            platformVelocity = currentPlatformRb.velocity;
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

        if (isGrounded)
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
        Vector3 currentHorizontal = new Vector3(currentVel.x, 0f, currentVel.z);

        Vector3 newHorizontal = Vector3.MoveTowards(
            currentHorizontal,
            targetHorizontalVel,
            acceleration * Time.fixedDeltaTime
        );

        rb.velocity = new Vector3(newHorizontal.x, currentVel.y, newHorizontal.z) + platformVelocity;

        if (visualRoot != null && moveDir.sqrMagnitude > 0.0001f)
        {
            Quaternion targetRot = Quaternion.LookRotation(moveDir, Vector3.up);
            visualRoot.rotation = Quaternion.Slerp(visualRoot.rotation, targetRot, rotationSpeed * Time.fixedDeltaTime);
        }
    }

    private void HandleJumpAndGravity()
    {
        Vector3 v = rb.velocity;

        // 점프 조건:
        // - 점프 버퍼가 살아있고
        // - (지면이거나 코요테 타임이 남아있으면) 점프 실행
        bool canJump = (jumpBufferCounter > 0f) && (isGrounded || coyoteCounter > 0f);

        if (canJump)
        {
            // 버퍼 소모
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

        // 중력 처리
        if (!isGrounded)
        {
            float g = gravity;

            // 낙하 가속
            if (v.y < 0f) g *= fallMultiplier;

            // 짧게 누르면 낮게 점프(상승 중 버튼을 놓으면 더 빨리 떨어지게)
            if (!jumpHeld && v.y > 0f)
                g *= jumpCutMultiplier;

            v.y -= g * Time.fixedDeltaTime;
            rb.velocity = v;
        }
        else
        {
            // 바닥에 붙는 힘(내리막 공중부양 방지)
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

        float sphereRadius = capsule.radius * 0.85f;
        Vector3 origin = rb.position + Vector3.up * (capsule.height * 0.5f - sphereRadius);

        if (Physics.SphereCast(origin, sphereRadius, Vector3.up, out RaycastHit hit,
            ceilingCheckDistance, groundLayer, QueryTriggerInteraction.Ignore))
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


