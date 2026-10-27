using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
public class MouseController : MonoBehaviour
{
    [Header("移动与跳跃参数")]
    public float walkSpeed = 2.5f;
    public float turnSmoothTime = 0.1f;
    public float jumpForce = 7f;
    public float gravityMultiplier = 2.5f;

    [Header("滞空手感 (Hang Time)")]
    [Tooltip("当垂直速度绝对值低于此值时，触发滞空感")]
    public float hangVelocityThreshold = 1.5f;
    [Tooltip("滞空期间的重力缩放系数 (0.2表示只受20%的重力，越大落得越快)")]
    public float hangGravityScale = 0.2f;

    [Header("冲刺 (Dash) 参数")]
    public float dashSpeed = 12f;
    public float dashDuration = 0.2f;
    public float dashCooldown = 0.8f;
    private float dashTimeCounter;
    private float cooldownCounter;
    private bool isDashing = false;

    [Header("模型与视觉引用")]
    public Transform bodyModel;
    public Transform cameraTransform;

    [Header("挤压拉伸")]
    public float stretchAmount = 1.4f;
    public float squashAmount = 0.7f;
    public float visualAnimSpeed = 15f;

    private Rigidbody rb;
    private Vector3 bodyOriginalScale;
    private bool isGrounded;

    private float moveInputH;
    private float moveInputV;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        rb.useGravity = true;
        rb.interpolation = RigidbodyInterpolation.Interpolate;

        if (bodyModel != null)
            bodyOriginalScale = bodyModel.localScale;

        if (cameraTransform == null && Camera.main != null)
            cameraTransform = Camera.main.transform;
    }

    void Update()
    {
        moveInputH = Input.GetAxisRaw("Horizontal");
        moveInputV = Input.GetAxisRaw("Vertical");

        CheckGround();
        HandleDash();
        HandleJump();
        HandleVisuals();
    }

    void FixedUpdate()
    {
        HandleMovement();
    }

    void CheckGround()
    {
        isGrounded = Physics.Raycast(transform.position + Vector3.up * 0.1f, Vector3.down, 0.35f);

        if (!isGrounded)
        {
            // 【核心修改：滞空重力逻辑】
            float currentGravityMultiplier = gravityMultiplier;

            // 如果垂直速度的绝对值很小（说明到了最高点附近）
            if (Mathf.Abs(rb.velocity.y) < hangVelocityThreshold)
            {
                // 重力变小，产生滞空感
                currentGravityMultiplier = gravityMultiplier * hangGravityScale;
            }

            // 施加计算后的重力
            rb.AddForce(Vector3.down * currentGravityMultiplier, ForceMode.Acceleration);
        }
    }

    void HandleDash()
    {
        if (cooldownCounter > 0) cooldownCounter -= Time.deltaTime;

        if (Input.GetMouseButtonDown(1) && cooldownCounter <= 0 && isGrounded)
        {
            isDashing = true;
            dashTimeCounter = dashDuration;
            cooldownCounter = dashCooldown + dashDuration;
        }

        if (isDashing)
        {
            dashTimeCounter -= Time.deltaTime;
            if (dashTimeCounter <= 0) isDashing = false;
        }
    }

    void HandleMovement()
    {
        Vector3 direction = new Vector3(moveInputH, 0f, moveInputV).normalized;
        float currentSpeed = isDashing ? dashSpeed : walkSpeed;

        if (direction.magnitude < 0.1f && !isDashing)
        {
            rb.velocity = new Vector3(0, rb.velocity.y, 0);
            rb.angularVelocity = Vector3.zero;
            return;
        }

        if (direction.magnitude >= 0.1f)
        {
            float moveAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + cameraTransform.eulerAngles.y;
            Vector3 moveDir = Quaternion.Euler(0f, moveAngle, 0f) * Vector3.forward;

            float lookAngle;
            if (moveInputV < 0)
                lookAngle = Mathf.Atan2(direction.x, -direction.z) * Mathf.Rad2Deg + cameraTransform.eulerAngles.y;
            else
                lookAngle = moveAngle;

            Quaternion targetRotation = Quaternion.Euler(0f, lookAngle, 0f);
            rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRotation, Time.fixedDeltaTime * 10f));

            rb.velocity = new Vector3(moveDir.x * currentSpeed, rb.velocity.y, moveDir.z * currentSpeed);
        }
        else if (isDashing)
        {
            rb.velocity = transform.forward * currentSpeed + new Vector3(0, rb.velocity.y, 0);
        }
    }

    void HandleJump()
    {
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded && !isDashing)
        {
            rb.velocity = new Vector3(rb.velocity.x, jumpForce, rb.velocity.z);
        }
    }

    void HandleVisuals()
    {
        if (bodyModel == null) return;

        Vector3 targetScale = bodyOriginalScale;

        if (isDashing)
        {
            targetScale = new Vector3(bodyOriginalScale.x * 0.7f, bodyOriginalScale.y * 1.5f, bodyOriginalScale.z * 0.7f);
        }
        else if (!isGrounded)
        {
            // 【修复动画闪烁】：利用我们设定的滞空阈值来区分上升、滞空、下落
            if (rb.velocity.y > hangVelocityThreshold)
            {
                // 上升阶段：拉长
                targetScale = new Vector3(bodyOriginalScale.x / stretchAmount, bodyOriginalScale.y * stretchAmount, bodyOriginalScale.z / stretchAmount);
            }
            else if (rb.velocity.y < -hangVelocityThreshold)
            {
                // 下落阶段：压扁
                targetScale = new Vector3(bodyOriginalScale.x * stretchAmount, bodyOriginalScale.y / stretchAmount, bodyOriginalScale.z * squashAmount);
            }
            else
            {
                // 滞空阶段：保持平滑，不闪烁
                targetScale = bodyOriginalScale;
            }
        }
        else
        {
            float breathing = 1f + Mathf.Sin(Time.time * 5f) * 0.02f;
            targetScale = new Vector3(bodyOriginalScale.x * breathing, bodyOriginalScale.y / breathing, bodyOriginalScale.z * breathing);
        }

        bodyModel.localScale = Vector3.Lerp(bodyModel.localScale, targetScale, Time.deltaTime * visualAnimSpeed);
    }
}