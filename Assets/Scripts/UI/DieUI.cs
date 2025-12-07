using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class DieUI : MonoBehaviour
{
    [Header("渐显设置")]
    [SerializeField] private float fadeDuration = 1.0f;  // 渐显持续时间
    [SerializeField] private float startAlpha = 0f;      // 起始透明度
    [SerializeField] private float endAlpha = 1f;        // 结束透明度

    private CanvasGroup canvasGroup;
    private PlayerController playerInstance;
    private Coroutine coroutine;

    private IEnumerator FadeIn()
    {
        float elapsedTime = 0f;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / fadeDuration);

            canvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, EaseInOut(t));

            yield return null;
        }

        canvasGroup.alpha = endAlpha;
    }

    private float EaseInOut(float t)
    {
        return t < 0.5f ? 2 * t * t : -1 + (4 - 2 * t) * t;
    }

    public void StartDie()
    {
        StartCoroutine(FadeIn());
    }
    public void OnButtonRestart()
    {
        if (playerInstance!=null&&playerInstance.gameObject!=null)
        {
            Destroy(playerInstance.gameObject);
        }
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    public void OnButtonExit()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
    // Start is called before the first frame update
    void Start()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        canvasGroup.alpha = startAlpha;
        if(PlayerController.Instance != null)
        {
            playerInstance = PlayerController.Instance;
        }
        playerInstance.OnDeath += StartDie;
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
