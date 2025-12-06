using System.Collections;
using TMPro;
using UnityEngine;

public class TextController : MonoBehaviour
{
    [Header("Text组件")]
    [SerializeField] private TextMeshPro text;

    [Header("淡入淡出设置")]
    [SerializeField] private float fadeDuration = 0.5f;
    [SerializeField] [Range(0f, 1f)] private float targetAlpha = 1f;

    private Coroutine fadeCoroutine;

    private void Awake()
    {
        if (text == null)
        {
            text = GetComponentInChildren<TextMeshPro>();
        }
    }

    private void Start()
    {
        if (text == null)
        {
            Debug.LogWarning($"[{nameof(TextController)}] 未找到 TextMeshPro 组件。", this);
            return;
        }

        // 初始隐藏
        SetAlpha(0f);
        text.enabled = false;
    }

    // 当玩家进入触发器时淡入，离开时淡出
    private void OnTriggerEnter(Collider other)
    {
        if (other == null || text == null) return;

        var pc = other.GetComponent<PlayerController>();
        if (pc == null) return; // 只对玩家响应

        StartFadeIn();
    }

    private void OnTriggerExit(Collider other)
    {
        if (other == null || text == null) return;

        var pc = other.GetComponent<PlayerController>();
        if (pc == null) return;

        StartFadeOut();
    }

    private void StartFadeIn()
    {
        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
        text.enabled = true;
        fadeCoroutine = StartCoroutine(FadeCoroutine(targetAlpha, false));
    }

    private void StartFadeOut()
    {
        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
        // 在淡出完成后禁用 text 组件以节省渲染开销
        fadeCoroutine = StartCoroutine(FadeCoroutine(0f, true));
    }

    private IEnumerator FadeCoroutine(float goalAlpha, bool disableAfter)
    {
        float startAlpha = text.color.a;
        if (Mathf.Approximately(startAlpha, goalAlpha))
        {
            if (disableAfter && Mathf.Approximately(goalAlpha, 0f))
                text.enabled = false;
            fadeCoroutine = null;
            yield break;
        }

        float time = 0f;
        while (time < fadeDuration)
        {
            time += Time.deltaTime;
            float t = Mathf.Clamp01(time / fadeDuration);
            float a = Mathf.Lerp(startAlpha, goalAlpha, t);
            SetAlpha(a);
            yield return null;
        }

        SetAlpha(goalAlpha);
        if (disableAfter && Mathf.Approximately(goalAlpha, 0f))
        {
            text.enabled = false;
        }

        fadeCoroutine = null;
    }

    private void SetAlpha(float a)
    {
        var c = text.color;
        c.a = Mathf.Clamp01(a);
        text.color = c;
    }
}
