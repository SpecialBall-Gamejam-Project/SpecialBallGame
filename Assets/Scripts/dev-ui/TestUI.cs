using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestUI : MonoBehaviour
{
    // Start is called before the first frame update
    private PlayerController pc;
    void Start()
    {
        pc=PlayerController.Instance;
        Invoke("ModifyInflationScale", 1f);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    void ModifyInflationScale()
    {
        pc.TriggerDeath();
    }
}
