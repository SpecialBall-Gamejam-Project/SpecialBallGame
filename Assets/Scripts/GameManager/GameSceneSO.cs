using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.AddressableAssets;

[CreateAssetMenu(fileName = "GameSceneSO", menuName = "Game Manager/Game Scene SO")]
public class GameSceneSO : ScriptableObject
{
    public SceneType sceneType;
    public AssetReference sceneReference;
    public void LoadScene(LoadSceneMode loadSceneMode = LoadSceneMode.Single)
    {
        if (sceneReference == null)
        {
            Debug.LogError("Scene reference is null.");
            return;
        }
        Addressables.LoadSceneAsync(sceneReference, loadSceneMode).Completed += handle =>
        {
            if (handle.Status != UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationStatus.Succeeded)
            {
                Debug.LogError($"Failed to load scene: {sceneReference.RuntimeKey}");
            }
        };
    }
}
