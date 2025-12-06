using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WindZoneVFX : MonoBehaviour
{
    // 风粒子特效
    public ParticleSystem windParticleSystem;
    // 在游戏开始时播放风粒子特效
    // Start is called before the first frame update
    void Start()
    {
        if(windParticleSystem != null)
            windParticleSystem.Play();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
