using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AscendAndDescend : MonoBehaviour
{
    public bool isAscend = true;
    public float increaseAmount = 0.25f;
    public float decreaseAmount = 0.25f;

    private PlayerController playerInstance; 
    private void OnTriggerEnter(Collider other)
    {
        if (isAscend)
        {
            playerInstance.inflationScale += increaseAmount;
        }
        else
        {
            playerInstance.inflationScale -= decreaseAmount;
        }
        Destroy(gameObject);
    }
    // Start is called before the first frame update
    void Start()
    {
        if (PlayerController.Instance != null) 
        { 
            playerInstance = PlayerController.Instance;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
