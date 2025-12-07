using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "SceneLoadEvent", menuName = "Game Events/Scene Load Event")]
public class SceneLoadEvent : ScriptableObject
{
    public UnityAction<GameSceneSO, Vector3, bool> LoadRequestEvent;
    public void RaiseLoadRequestEvent(GameSceneSO scene, Vector3 position, bool fade)
    {
        Debug.Log($"SceneLoadEvent: Raising load request for scene {scene.sceneType} to position {position} with fade {fade}");
        LoadRequestEvent?.Invoke(scene, position, fade);
    }
}
