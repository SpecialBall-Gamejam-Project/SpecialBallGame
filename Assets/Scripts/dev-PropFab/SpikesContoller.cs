using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpikesContoller : MonoBehaviour
{
    // 伤害值范围0-1
    [SerializeField]
    [Range(0, 1)] private float  damageValue = 0.1f;
    [SerializeField] private float duration = 1.0f;

    private Coroutine damageCoroutine;
    private bool playerInside = false;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //检测到玩家进入触发器,调用玩家单例的伤害函数
    private void OnTriggerEnter(Collider other)
    {
        if (other == null) return;
        if (other.CompareTag("Player"))
        {
            // 立即造成一次伤害
            PlayerController.Instance?.InflationAdd(-damageValue);

            // 标记玩家在触发器内并启动定时伤害协程（如果尚未启动）
            playerInside = true;
            if (damageCoroutine == null)
            {
                damageCoroutine = StartCoroutine(PeriodicDamage());
            }
        }
    }

    //检测到玩家停留在触发器内,每隔duration秒调用玩家单例的伤害函数
    private IEnumerator PeriodicDamage()
    {
        while (playerInside)
        {
            yield return new WaitForSeconds(duration);

            if (!playerInside) break;

            var pc = PlayerController.Instance;
            if (pc == null) break;

            // 如果玩家已死亡或实例为空则停止
            if (pc.IsDead) break;

            pc.InflationAdd(-damageValue);
        }

        damageCoroutine = null;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other == null) return;
        if (other.CompareTag("Player"))
        {
            // 玩家离开触发器，停止定时伤害
            playerInside = false;
            if (damageCoroutine != null)
            {
                StopCoroutine(damageCoroutine);
                damageCoroutine = null;
            }
        }
    }

    private void OnDisable()
    {
        // 组件被禁用时确保协程停止
        playerInside = false;
        if (damageCoroutine != null)
        {
            StopCoroutine(damageCoroutine);
            damageCoroutine = null;
        }
    }
}
