using System.Collections.Generic;
using UnityEngine;

public class TailSkeletonController : MonoBehaviour
{
    [Header("骨骼拖入 (按根到尖的顺序，从屁股到尾巴尖)")]
    public List<Transform> tailBones = new List<Transform>();

    [Header("摆动参数")]
    public float idleSwaySpeed = 2f;      // 静止时的摆动速度
    public float moveSwaySpeed = 10f;     // 移动时的摆动速度
    public float maxSwayAngle = 25f;      // 最大摆动角度
    public float waveDelay = 0.6f;        // 波浪延迟 (每节骨骼之间的延迟，越大越像鞭子)
    public float smoothTime = 8f;         // 平滑度

    [Header("轴向修正 (极其重要！)")]
    [Tooltip("如果尾巴像麻花一样扭曲或者乱飞，勾选或取消这个试试")]
    public bool useXAxisForSway = false;

    private Rigidbody rb;
    private float[] boneOffsets;

    void Start()
    {
        // 自动寻找父物体（Player）上的刚体，用来判断移动速度
        rb = GetComponentInParent<Rigidbody>();

        // 给每根骨骼生成一个随机初始相位，避免它们完全同步摆动
        boneOffsets = new float[tailBones.Count];
        for (int i = 0; i < tailBones.Count; i++)
        {
            boneOffsets[i] = Random.Range(0f, 10f);
        }
    }

    void LateUpdate() // 使用LateUpdate保证在移动逻辑之后执行
    {
        if (tailBones.Count == 0 || rb == null) return;

        // 获取老鼠当前的水平移动速度
        float currentSpeed = new Vector3(rb.velocity.x, 0, rb.velocity.z).magnitude;
        bool isGrounded = Mathf.Abs(rb.velocity.y) < 0.1f;

        // 根据速度动态混合摆动速度（静止慢，跑动快）
        float currentSpeedValue = Mathf.Lerp(idleSwaySpeed, moveSwaySpeed, currentSpeed / 5f);

        // 跳跃时，尾巴整体向上扬起（如果你发现跳跃时尾巴朝下钻，把这里的 -20f 改成 20f）
        float jumpPitch = isGrounded ? 0f : -20f;

        for (int i = 0; i < tailBones.Count; i++)
        {
            // 核心逻辑：每一节骨骼比上一节多一点点延迟，产生优美的波浪
            float phase = Time.time * currentSpeedValue + (i * waveDelay) + boneOffsets[i];

            // 计算当前骨骼需要旋转的角度
            float swayAngle = Mathf.Sin(phase) * maxSwayAngle;

            // 骨骼的基础旋转
            Quaternion targetRot;
            if (useXAxisForSway)
            {
                targetRot = Quaternion.Euler(jumpPitch, 0, swayAngle);
            }
            else
            {
                targetRot = Quaternion.Euler(jumpPitch, swayAngle, 0);
            }

            // 平滑应用旋转
            tailBones[i].localRotation = Quaternion.Slerp(tailBones[i].localRotation, targetRot, Time.deltaTime * smoothTime);
        }
    }
}