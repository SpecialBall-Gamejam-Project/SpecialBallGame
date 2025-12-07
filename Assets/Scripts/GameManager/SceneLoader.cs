using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.AddressableAssets;
using System.Net.NetworkInformation;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using System;

public class SceneLoader : MonoBehaviour
{
    public Transform playerTrans;
    public SceneLoadEvent LoadEvent;
    public GameSceneSO firstLoadScene;
    public GameSceneSO secondLoadScene;

    private GameSceneSO currentScene;
    private GameSceneSO sceneToLoad;
    private Vector3 positionToGo;
    private bool fadeScreen;

    private void Awake()
    {
        currentScene = firstLoadScene;
        currentScene.sceneReference.LoadSceneAsync(LoadSceneMode.Additive);
    }

    private void OnEnable()
    {
        LoadEvent.LoadRequestEvent += OnLoadRequsetEvent;
    }
    private void OnDisable()
    {
        LoadEvent.LoadRequestEvent -= OnLoadRequsetEvent;
    }

    private void OnLoadRequsetEvent(GameSceneSO scene, Vector3 position, bool fade)
    {
        sceneToLoad = scene;
        positionToGo = position;
        fadeScreen = fade;

        Debug.Log($"SceneLoader: Received load request for scene {sceneToLoad.sceneType} to position {positionToGo} with fade {fadeScreen}");
        StartCoroutine(UnLoadePreviousScene());
    }

    private IEnumerator UnLoadePreviousScene()
    {
        yield return new WaitForSeconds(1.0f);
        if(currentScene != null)
        {
            Debug.Log($"SceneLoader: Unloading current scene {currentScene.sceneType}");
            
            yield return currentScene.sceneReference.UnLoadScene();
        }
        LoadNewScene();
    }

    private void LoadNewScene()
    {
        if(sceneToLoad != null)
        {
            Debug.Log($"SceneLoader: Loading new scene {sceneToLoad.sceneType}");
            var loadingOption =  sceneToLoad.sceneReference.LoadSceneAsync(LoadSceneMode.Additive);
            loadingOption.Completed += OnLoadCompleted;
        }
    }

    private void OnLoadCompleted(AsyncOperationHandle<SceneInstance> obj)
    {
        playerTrans.position = positionToGo;
        currentScene = sceneToLoad;
    }
}
