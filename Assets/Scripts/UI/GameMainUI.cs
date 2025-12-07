using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameMainUI : MonoBehaviour
{
    private PlayerController playerInstance;
    // Start is called before the first frame update
    void Start()
    {
        if(PlayerController.Instance != null)
            playerInstance = PlayerController.Instance;
        playerInstance.OnDeath += () => gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
