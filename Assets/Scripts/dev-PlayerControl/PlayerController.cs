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
    [SerializeField] private float flyUpForce = 5.0f;   // 飞行升力
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
    public enum InflationState { Flying, Boosted, Normal, HalfPower, NoJump, NoMove }
    private InflationState currentState = InflationState.Normal;
    private bool canMove = true;
    private bool canJump = true;
    private float currentJumpForce = 0f;
    private float baseJumpForce = 0f;
    public InflationState CurrentState => currentState;

    // 水中状态相关参数
    [Space]
    [Header("水中状态")]
    [SerializeField] private float waterBuoyancyForce = 9.0f;         // 在水中无法沉入时的向上加速度
    [SerializeField] private float waterSinkForce = 4.0f;             // 在水中可沉时的向下加速度
    [SerializeField] private float waterMoveSpeedMultiplier = 0.6f;   // 水中水平移动速度系数
    private bool inWater = false;
    private Collider currentWater = null;
    private float originalDrag = 0f;

    // 风域相关
    [Space]
    [Header("风域设置")]
    [SerializeField] private float windForce = 10f;               // 风力强度
    [SerializeField] private bool windAffectsVertical = false;    // 风是否影响垂直分量
    private bool inWind = false;
    private Collider currentWind = null;
    private Vector3 windDirection = Vector3.zero;

    //组件
    //------------------------------
    [Space]
    [SerializeField] private Camera cam;
    private Rigidbody rb;
    private Collider col;
    
    [Space]
    [Header("死亡状态")]
    public ParticleSystem deathVFX;//死亡VFX动画
    // 死亡事件，外部订阅
    public event Action OnDeath;
    private bool isDead = false;
    public bool IsDead => isDead;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();
        if (rb != null) originalDrag = rb.drag;
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
        // 如果已死亡，屏蔽所有输入与状态更新
        if (isDead) return;

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
        // 如果已死亡，停止所有物理/控制逻辑（保留死后特效）
        if (isDead) return;

        //小球滚动逻辑（摄像机相对方向）
        //------------------------------
        if (rb == null) return;

        // 更新接地状态（从球体中心向下射线）
        Vector3 origin = transform.position;
        isGrounded = Physics.Raycast(origin, Vector3.down, groundCheckDistance, groundLayers);

        // 风力优先施加（风应持续作用于刚体）
        if (inWind && currentWind != null)
        {
            rb.AddForce(windDirection * windForce, ForceMode.Force);
        }

        // 如果处于飞行状态：持续提供上升力，同时仍然可以滚动控制，但不能跳跃
        if (isFlying)
        {
            // 允许滚动
            if (canMove)
            {
                rb.AddForce(moveDir * speed, ForceMode.Force);
            }

            // 施加飞行升力
            rb.AddForce(Vector3.up * flyUpForce, ForceMode.Acceleration);

            // 调试
            Debug.Log($"Flying: inflation={_inflationScale:F2}, upForce={flyUpForce:F2}");
            return;
        }


        // 跳跃请求
        if (jumpRequested && isGrounded && canJump && currentJumpForce > 0f)
        {
            // 记录跳跃方向并施加跳跃
            Vector3 horizontalImpulse = jumpDirection * currentJumpForce;
            Vector3 verticalImpulse = Vector3.up * currentJumpForce;
            Vector3 impulse = verticalImpulse + horizontalImpulse;
            rb.AddForce(impulse, ForceMode.Impulse);

            jumpRequested = false;
            // 跳起后标记未接地
            isGrounded = false;
            return;
        }

        // 水中逻辑
        if (inWater)
        {
            HandleWaterMotion();
            return;
        }

        // 移动 -无法移动时不施加水平推进力并尽量清除水平速度
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

    private void OnTriggerEnter(Collider other)
    {
        if (other == null) return;
        if (other.CompareTag("Water"))
        {
            inWater = true;
            currentWater = other;
            // 增加阻尼使得水中行为更符合预期（暂存原始阻尼）
            if (rb != null)
            {
                rb.drag = Mathf.Max(rb.drag, 2f);
            }
            //Debug.Log("Entered Water");
        }
        else if (other.CompareTag("Wind"))
        {
            // 进入风域：记录风对象与方向（风向为风域的本地 +Z）
            inWind = true;
            currentWind = other;
            windDirection = other.transform.forward;
            if (!windAffectsVertical)
            {
                windDirection.y = 0f;
            }
            if (windDirection.sqrMagnitude > 0.001f) windDirection.Normalize();
            //Debug.Log($"Entered Wind - direction={windDirection}");
        }
        else if(other.CompareTag("Death"))
        {
            // 进入死亡区域：触发死亡
            TriggerDeath();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other == null) return;
        if (other.CompareTag("Water") && other == currentWater)
        {
            inWater = false;
            currentWater = null;
            // 恢复阻尼
            if (rb != null)
            {
                rb.drag = originalDrag;
            }
            //Debug.Log("Exited Water");
        }
        else if (other.CompareTag("Wind") && other == currentWind)
        {
            // 离开风域：停止受该风影响
            inWind = false;
            currentWind = null;
            windDirection = Vector3.zero;
            //Debug.Log("Exited Wind");
        }
    }

    // 对外调用：触发死亡（道具等外部对象调用此方法）
    public void TriggerDeath()
    {
        if (isDead) return;
        HandleDeath();
    }

    // 内部处理死亡逻辑
    private void HandleDeath()
    {
        if (isDead) return;
        isDead = true;

        // 广播死亡事件给外部订阅者（比如UI）
        OnDeath?.Invoke();

        // 停止并禁用碰撞体以避免再触发交互
        if (col != null) col.enabled = false;

        // 停止物理运动并使刚体不再受力
        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
        }

        // 播放死亡特效（实例化副本以便脱离 Player 对象的生命周期）
        if (deathVFX != null)
        {
            var vfxInstance = Instantiate(deathVFX, transform.position, Quaternion.identity);
            vfxInstance.Play();
            // 自动销毁特效对象（基于主系统时长）
            var lifetime = 0f;
            try
            {
                lifetime = vfxInstance.main.duration + vfxInstance.main.startLifetime.constantMax;
            }
            catch { lifetime = 5f; }
            Destroy(vfxInstance.gameObject, lifetime + 0.1f);
        }

        // 隐藏模型
        var rends = GetComponentsInChildren<Renderer>(true);
        foreach (var r in rends)
        {
            r.enabled = false;
        }

        // 禁用本脚本以进一步保证不会再处理输入（可选：保留 isDead 以在需要时查询）
        enabled = false;
    }

    private void OnDisable()
    {
        // 如果脚本被禁用但对象还存在，确保 inWind/inWater 标记在必要时清理（防止残留）
        inWind = false;
        inWater = false;
    }

    // 水域相关逻辑
    //------------------------------
    // 判断当前是否可以沉入水底
    // -仅在 HalfPower 或 NoJump 时允许沉
    private bool IsSinkableInWater()
    {
        return currentState == InflationState.HalfPower || currentState == InflationState.NoJump;
    }
    // 处理在水中的运动：根据状态决定上浮或下沉，并调整水平控制
    private void HandleWaterMotion()
    {
        if (rb == null) return;

        bool sinkable = IsSinkableInWater();

        // 水中水平控制（允许，但速度受限）
        if (canMove)
        {
            rb.AddForce(moveDir * speed * waterMoveSpeedMultiplier, ForceMode.Force);
        }

        if (sinkable)
        {
            // 允许沉：施加向上加速度实现缓入水效果，同时减少上浮阻力
            rb.AddForce(Vector3.up * waterSinkForce, ForceMode.Acceleration);
            // 减弱向上的速度（防止短时间反弹）
            Debug.Log($"In Water - Sinkable ({currentState}), sinking...");
        }
        else
        {
            // 不允许沉：施加浮力向上，尽量抵消下落速度
            rb.AddForce(Vector3.up * waterBuoyancyForce, ForceMode.Acceleration);
            // 如果有向下速度，做阻尼减少沉降
            if (rb.velocity.y < 0f)
            {
                rb.velocity = new Vector3(rb.velocity.x, rb.velocity.y * 0.3f, rb.velocity.z);
            }
            Debug.Log($"In Water - Buoyant ({currentState}), floating");
        }
    }
}
