using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Barometer : MonoBehaviour
{

    [Header("Pressure(%)")]
    [SerializeField] private float maxPressure = 2.0f;
    [SerializeField] private float currentPressure = 0.5f;
    [SerializeField] private float targetPressure = 1.0f;

    [Header("Threshold")]
    [SerializeField] private float flyThreshold = 1.2f; // 飞行阈值（这里的阈值均为下限）
    [SerializeField] private float boostThreshold = 0.9f; // 加强阈值
    [SerializeField] private float normalThreshold = 0.7f; // 正常阈值
    [SerializeField] private float halfPowerThreshold = 0.5f; // 半功率阈值
    [SerializeField] private float noJumpThreshold = 0.3f; // 无跳跃阈值

    [Header("UI")]
    [SerializeField] private Image barometerFillImage;
    [SerializeField] private TMPro.TextMeshProUGUI barometerText;

    [Header("Animation")]
    //[SerializeField] private Gradient barometerGradient;
    [SerializeField] private float smoothSpeed = 10f;

    void Awake()
    {
        if(barometerFillImage == null)
            barometerFillImage=GetComponent<Image>();
        if(barometerText == null)
            barometerText=GetComponentInChildren<TMPro.TextMeshProUGUI>();
    }

    void Start()
    {
        if (PlayerController.Instance != null)
        {
            targetPressure = PlayerController.Instance.inflationScale;
            currentPressure = targetPressure;
            //Debug.Log("target:"+targetPressure+" current:"+currentPressure);
        }
        //Debug.Log("target:" + targetPressure + " current:" + currentPressure);
        UpdateBarometer();
    }

    void Update()
    {
        if (PlayerController.Instance != null)
        {
            targetPressure = PlayerController.Instance.inflationScale;
        }
        if (Mathf.Abs(currentPressure - targetPressure) > 0.01f)
        {
            currentPressure=Mathf.Lerp(currentPressure, targetPressure, smoothSpeed*Time.deltaTime);
            UpdateBarometer();
        }
        //Debug.Log("target:" + targetPressure + " current:" + currentPressure);

    }

    void UpdateBarometer()
    {
        if (barometerFillImage != null)
        {
            barometerFillImage.fillAmount = currentPressure / maxPressure;
            if (currentPressure < noJumpThreshold)
                barometerFillImage.color = Color.white;
            else if (currentPressure < halfPowerThreshold)
                barometerFillImage.color = Color.green;
            else if (currentPressure < normalThreshold)
                barometerFillImage.color = Color.yellow;
            else if (currentPressure < boostThreshold)
                barometerFillImage.color = new Color(1f, 165f / 255f, 0);
            else if (currentPressure < flyThreshold)
                barometerFillImage.color = Color.red;
            else
                barometerFillImage.color = Color.red;
        }
        if(barometerText != null)
        {
            barometerText.text = Mathf.CeilToInt(currentPressure*100).ToString();
        }
    }
}
