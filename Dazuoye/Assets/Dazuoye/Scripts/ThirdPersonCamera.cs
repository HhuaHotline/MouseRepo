using UnityEngine;

public class ThirdPersonCamera : MonoBehaviour
{
    [Header("目标")]
    public Transform target;            // 拖入 Player

    [Header("鼠标控制")]
    public float mouseSensitivity = 3f;
    public float rotationSmoothTime = 0.12f;
    private float yaw;
    private float pitch = 5f;

    [Header("视角限制 (左右偏转)")]
    [Tooltip("左右最大偏转角 (默认左右各60度)")]
    public float maxYawAngle = 60f;

    [Header("视角限制 (远景上下)")]
    public float farMinPitch = -10f;    // 远景最小俯仰角 
    public float farMaxPitch = 45f;     // 远景最大俯仰角 

    [Header("视角限制 (近景上下)")]
    public float closeMinPitch = -15f;  // 近景最小俯仰角 (平视往上15度)
    public float closeMaxPitch = 15f;   // 近景最大俯仰角 (平视往下15度)

    [Header("超限拉扯感 (Over-limit Lean)")]
    public float leanSpeed = 2.5f;      // 到达极限后，身体被拉扯转动的速度

    [Header("视角状态 (按Shift切换)")]
    public bool isCloseView = false;

    [Header("远景参数 (后上方)")]
    public float farDistance = 4f;
    public float farHeight = 1.5f;

    [Header("近景参数 (正后方)")]
    public float closeDistance = 1.5f;
    public float closeHeight = 0.8f;

    private float currentDistance;
    private float currentHeight;
    private Vector3 currentVelocity;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        currentDistance = farDistance;
        currentHeight = farHeight;

        if (target != null) yaw = target.eulerAngles.y;
    }

    void Update()
    {
        if (!target) return;

        // 监听 Shift 键切换
        if (Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift))
        {
            isCloseView = !isCloseView;
            if (isCloseView)
            {
                yaw = target.eulerAngles.y;
                pitch = 0f; // 切近景时默认平视                 
            }
        }

        float targetDistance = isCloseView ? closeDistance : farDistance;
        float targetHeight = isCloseView ? closeHeight : farHeight;

        currentDistance = Mathf.Lerp(currentDistance, targetDistance, Time.deltaTime * 10f);
        currentHeight = Mathf.Lerp(currentHeight, targetHeight, Time.deltaTime * 10f);

        // ================= 核心修改：获取鼠标输入 =================
        yaw += Input.GetAxis("Mouse X") * mouseSensitivity;
        pitch -= Input.GetAxis("Mouse Y") * mouseSensitivity;

        // ================= 【修复】动态应用上下视角限制 =================
        if (isCloseView)
        {
            pitch = Mathf.Clamp(pitch, closeMinPitch, closeMaxPitch);
        }
        else
        {
            pitch = Mathf.Clamp(pitch, farMinPitch, farMaxPitch);
        }

        // ================= 左右视角限制 (超限拉扯) =================
        float targetBaseYaw = target.eulerAngles.y;
        float yawOffset = Mathf.DeltaAngle(targetBaseYaw, yaw);

        if (Mathf.Abs(yawOffset) > maxYawAngle)
        {
            float excess = Mathf.Abs(yawOffset) - maxYawAngle;
            float sign = Mathf.Sign(yawOffset);

            target.Rotate(Vector3.up, sign * excess * Time.deltaTime * leanSpeed, Space.World);

            targetBaseYaw = target.eulerAngles.y;
            yawOffset = Mathf.DeltaAngle(targetBaseYaw, yaw);
        }

        yawOffset = Mathf.Clamp(yawOffset, -maxYawAngle, maxYawAngle);
        yaw = targetBaseYaw + yawOffset;
        // ================================================================
    }

    void LateUpdate()
    {
        if (!target) return;

        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0);

        Vector3 targetPosition = target.position
                                 - (rotation * Vector3.forward * currentDistance)
                                 + (Vector3.up * currentHeight);

        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref currentVelocity, rotationSmoothTime);

        if (isCloseView)
        {
            // 【修复】：去掉了之前多余的偏移角，直接使用限制后的 pitch
            transform.rotation = Quaternion.Euler(pitch, yaw, 0);
        }
        else
        {
            transform.LookAt(target.position + Vector3.up * 0.5f);
        }
    }
}