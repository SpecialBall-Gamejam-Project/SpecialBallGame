using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeleportPoint : MonoBehaviour
{
    public SceneLoadEvent loadEvent;
    public GameSceneSO sceneToGo;
    public Vector3 positionToGo;

    //如果玩家进入触发器,则触发传送事件
    
    private void OnTriggerEnter(Collider other)
    {
        if (other == null) return;
        if (other.CompareTag("Player"))
        {
            TriggerAction();
        }
    }
    public void TriggerAction()
    {
        Debug.Log($"TeleportPoint: Triggering teleport to scene {sceneToGo.sceneType} at position {positionToGo}");
        loadEvent.RaiseLoadRequestEvent(sceneToGo, positionToGo, true);
    }
}
