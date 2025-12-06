using System.Collections;
using UnityEngine;

/// <summary>
/// 按钮控制器：检测玩家碰撞并在玩家处于 NoJump 状态时触发按下动画（Y 轴缩放到 0.4）。
/// 使用方法：将此脚本挂到按钮根对象上，指定 body（可为空，若为空则使用根 Transform）。
/// 根对象或 Body 子物体需要带 Collider（isTrigger 可选）以接收碰撞/触发事件。
/// </summary>
public class ButtonController : MonoBehaviour
{
    [Header("按钮")]
    [SerializeField] private Transform body;

    [Header("按下参数")]
    [SerializeField] private float pressedYScale = 0.4f;
    [SerializeField] private float animDuration = 0.1f;

    private Vector3 originalScale;
    private Coroutine animCoroutine;
    private bool isPressed = false;

    private void Awake()
    {
        if (body == null)
        {
            body = transform;
        }

        originalScale = body.localScale;
    }

    // 支持 Trigger 或 普通碰撞
    private void OnTriggerEnter(Collider other)
    {
        TryPressFromCollider(other);
    }

    private void OnCollisionEnter(Collision collision)
    {
        TryPressFromCollider(collision.collider);
    }

    private void OnTriggerExit(Collider other)
    {
        //TryReleaseFromCollider(other);
    }

    private void OnCollisionExit(Collision collision)
    {
        //TryReleaseFromCollider(collision.collider);
    }

    private void TryPressFromCollider(Collider other)
    {
        if (other == null) return;

        var pc = other.GetComponent<PlayerController>();
        if (pc == null) return;

        // 仅当玩家处于 NoJump 状态时才能按下按钮
        if (pc.CurrentState == PlayerController.InflationState.NoJump)
        {
            Press();
        }
    }

    private void TryReleaseFromCollider(Collider other)
    {
        if (other == null) return;

        var pc = other.GetComponent<PlayerController>();
        if (pc == null) return;

        //玩家离开时恢复按钮(暂时不用)
        if (isPressed)
        {
            Release();
        }
    }

    private void Press()
    {
        if (isPressed || body == null) return;
        isPressed = true;
        StartScaleAnimation(pressedYScale);
    }

    private void Release()
    {
        if (!isPressed || body == null) return;
        isPressed = false;
        StartScaleAnimation(originalScale.y);
    }

    private void StartScaleAnimation(float targetY)
    {
        if (animCoroutine != null) StopCoroutine(animCoroutine);
        animCoroutine = StartCoroutine(AnimateScaleY(targetY));
    }

    private IEnumerator AnimateScaleY(float targetY)
    {
        float elapsed = 0f;
        Vector3 start = body.localScale;
        Vector3 target = new Vector3(start.x, targetY, start.z);
        while (elapsed < animDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / animDuration);
            float y = Mathf.Lerp(start.y, target.y, t);
            body.localScale = new Vector3(start.x, y, start.z);
            yield return null;
        }
        body.localScale = target;
        animCoroutine = null;
    }
}