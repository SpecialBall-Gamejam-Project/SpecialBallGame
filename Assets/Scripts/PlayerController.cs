using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Rendering;
using UnityEngine;
//实现小球的滚动、跳跃功能
public class PlayerController : MonoBehaviour
{
    //玩家单例
    public static PlayerController Instance;

    //各参数
    //------------------------------
    [SerializeField] private float speed = 3.0f;
    private float moveHorizontal = 0.0f;
    private float moveVertical = 0.0f;
    Vector3 moveDir;
    // 地面检测与跳跃状态
    [SerializeField] private LayerMask groundLayers = ~0; // 默认所有层为地面
    private float groundCheckDistance = 0.1f;
    private bool isGrounded = true;
    private bool jumpRequested = false;
    private Vector3 jumpDirection = Vector3.zero; // 跳跃时的水平方向（在空中保持不变）
    [SerializeField] private  float jumpForce = 5.0f;
    //飞行相关参数
    [SerializeField] private float flyUpForce = 5.0f;   // 飞行向上持续力（可按充气量缩放）
    private bool isFlying = false;
    public bool IsFlying => isFlying;

    //篮球充气量
    [SerializeField] private float _inflationScale = 1.0f;
    public float inflationScale
    {
        get { return _inflationScale; }
        set { _inflationScale = value; }
    }
    // 充气阈值
    [Space]
    [Header("各状态阈值")]
    [SerializeField] private float flyThreshold = 1.2f; // 飞行阈值（这里的阈值均为下限）
    [SerializeField] private float boostThreshold = 0.9f; // 加强阈值
    [SerializeField] private float normalThreshold = 0.7f; // 正常阈值
    [SerializeField] private float halfPowerThreshold = 0.5f; // 半功率阈值
    [SerializeField] private float noJumpThreshold = 0.3f; // 无跳跃阈值

    // 状态相关
    private enum InflationState { Flying, Boosted, Normal, HalfPower, NoJump, NoMove }
    private InflationState currentState = InflationState.Normal;
    private bool canMove = true;
    private bool canJump = true;
    private float currentJumpForce = 0f;
    private float baseJumpForce = 0f;

    //组件
    //------------------------------
    [Space]
    [SerializeField] private Camera cam;
    private Rigidbody rb;
    private Collider col;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();
        if (cam==null)
        {
            cam = Camera.main;
        }
        if (col != null)
        {
            // 从碰撞体高度计算射线距离，确保检测到地面
            groundCheckDistance = col.bounds.extents.y + 0.05f;
        }
        //玩家单例
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if(Instance != this)
        {
            Destroy(gameObject);
        }

        baseJumpForce = jumpForce;
        currentJumpForce = baseJumpForce;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //获取移动输入
        moveHorizontal = Input.GetAxis("Horizontal");
        moveVertical = Input.GetAxis("Vertical");
        // 以摄像机的右和前为方向参考，但只在水平面上移动（忽略摄像机的上下倾斜）
        Vector3 camRight = cam.transform.right;
        Vector3 camForward = cam.transform.forward;
        camRight.y = 0f;
        camForward.y = 0f;
        camRight.Normalize();
        camForward.Normalize();
        // 合成输入方向（相对于摄像机）
        moveDir = camRight * moveHorizontal + camForward * moveVertical;
        // 防止斜向移动速度超过预期
        if (moveDir.sqrMagnitude > 1f) moveDir.Normalize();

        // 计算当前充气状态并切换行为（使用 switch）
        currentState = GetInflationState(_inflationScale);
        switch (currentState)
        {
            case InflationState.Flying:
                isFlying = true;
                canMove = true;
                canJump = false;
                currentJumpForce = 0f;
                break;
            case InflationState.Boosted: // > boostThreshold 到 flyThreshold
                isFlying = false;
                canMove = true;
                canJump = true;
                currentJumpForce = baseJumpForce * 2f;
                break;
            case InflationState.Normal: // > halfPowerThreshold 到 boostThreshold
                isFlying = false;
                canMove = true;
                canJump = true;
                currentJumpForce = baseJumpForce;
                break;
            case InflationState.HalfPower: // > normalThreshold 到 halfPowerThreshold
                isFlying = false;
                canMove = true;
                canJump = true;
                currentJumpForce = baseJumpForce * 0.5f;
                break;
            case InflationState.NoJump: // > noJumpThreshold 到 normalThreshold
                isFlying = false;
                canMove = true;
                canJump = false;
                currentJumpForce = 0f;
                break;
            case InflationState.NoMove: // <= noJumpThreshold
            default:
                isFlying = false;
                canMove = false;
                canJump = false;
                currentJumpForce = 0f;
                break;
        }

        // 不能跳时清除跳跃请求
        if (!canJump)
        {
            jumpRequested = false;
        }

        //跳跃输入（仅在可跳且接地时可请求）
        if (canJump && Input.GetKeyDown(KeyCode.Space))
        {
            // 只有在接地时可请求跳跃
            if (isGrounded)
            {
                jumpRequested = true;
                // 记录跳跃方向；若无方向输入则只垂直跳
                jumpDirection = (moveDir.sqrMagnitude > 0.005f) ? moveDir.normalized : Vector3.zero;
            }
        }
    }

    private void FixedUpdate()
    {
        //小球滚动逻辑（摄像机相对方向）
        //------------------------------
        if (rb == null) return;

        // 更新接地状态（从球体中心向下射线）
        Vector3 origin = transform.position;
        isGrounded = Physics.Raycast(origin, Vector3.down, groundCheckDistance, groundLayers);

        // 如果处于飞行状态：持续提供上升力，同时仍然可以滚动控制，但不能跳跃
        if (isFlying)
        {
            // 允许滚动
            if (canMove)
            {
                rb.AddForce(moveDir * speed, ForceMode.Force);
            }

            // 升力
            rb.AddForce(Vector3.up * flyUpForce, ForceMode.Acceleration);

            // 调试
            Debug.Log($"Flying: inflation={_inflationScale:F2}, upForce={flyUpForce:F2}");
            return;
        }

        // 优先处理跳跃请求（在 FixedUpdate 中实际施加冲量）
        if (jumpRequested && isGrounded && canJump && currentJumpForce > 0f)
        {
            // 跳跃方向在跳起瞬间固定，之后不可改变
            Vector3 horizontalImpulse = jumpDirection * currentJumpForce;
            Vector3 verticalImpulse = Vector3.up * currentJumpForce;
            Vector3 impulse = verticalImpulse + horizontalImpulse;
            rb.AddForce(impulse, ForceMode.Impulse);

            jumpRequested = false;
            // 跳起后立即标记未接地，防止在同一 FixedUpdate 再受地面移动影响
            isGrounded = false;
            return;
        }

        // 移动：当无法移动时不施加水平推进力并尽量清除水平速度
        if (canMove)
        {
            rb.AddForce(moveDir * speed, ForceMode.Force);
        }
        else
        {
            Vector3 vel = rb.velocity;
            rb.velocity = new Vector3(0f, vel.y, 0f);
        }
    }

    // 将充气量映射为离散状态
    private InflationState GetInflationState(float v)
    {
        // 阈值判断（阈值按大小配置： flyThreshold > boostThreshold > normalThreshold > halfPowerThreshold > noJumpThreshold）
        if (v > flyThreshold) return InflationState.Flying;          // > flyThreshold
        if (v > boostThreshold) return InflationState.Boosted;       // (boostThreshold, flyThreshold]
        if (v > normalThreshold) return InflationState.Normal;    // (normalThreshold, boostThreshold]
        if (v > halfPowerThreshold) return InflationState.HalfPower;    // (halfPowerThreshold, normalThreshold]
        if (v > noJumpThreshold) return InflationState.NoJump;       // (noJumpThreshold, halfPowerThreshold]
        return InflationState.NoMove;                                // <= noJumpThreshold
    }
}
