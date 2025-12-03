using System.Collections;
using System.Collections.Generic;
using UnityEditor.Rendering;
using UnityEngine;
//实现小球的滚动、跳跃功能
public class PlayerController : MonoBehaviour
{
    //各参数
    //------------------------------
    public float speed = 10.0f;
    float moveHorizontal = 0.0f;
    float moveVertical = 0.0f;
    //重力值-暂时搁置
    private float gavityScale = 1.0f;
    public float _gavityScale
    {
        get { return gavityScale; }
        set
        {
            gavityScale = value;
        }
    }

    //组件
    //------------------------------
    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //获取输入
        moveHorizontal = Input.GetAxis("Horizontal");
        moveVertical = Input.GetAxis("Vertical");
    }

    private void FixedUpdate()
    {
        //小球滚动逻辑（摄像机相对方向）
        //------------------------------
        Camera cam = Camera.main;
        if (cam == null || rb == null) return;

        // 以摄像机的右和前为方向参考，但只在水平面上移动（忽略摄像机的上下倾斜）
        Vector3 camRight = cam.transform.right;
        Vector3 camForward = cam.transform.forward;
        camRight.y = 0f;
        camForward.y = 0f;
        camRight.Normalize();
        camForward.Normalize();

        // 合成输入方向（相对于摄像机）
        Vector3 moveDir = camRight * moveHorizontal + camForward * moveVertical;

        // 防止斜向移动速度超过预期
        if (moveDir.sqrMagnitude > 1f) moveDir.Normalize();

        // 施加力让刚体移动，使用 ForceMode.Force 以受质量影响的正常物理效果
        rb.AddForce(moveDir * speed, ForceMode.Force);
    }
}
