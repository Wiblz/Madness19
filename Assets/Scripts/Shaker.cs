using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Cinemachine;


public class Shaker : MonoBehaviour {
    private CinemachineVirtualCamera cinemachineVirtualCamera;
    private BulletHandler bulletHandler;

    private float shakeTime;

    void Start() {
        cinemachineVirtualCamera = GetComponent<CinemachineVirtualCamera>();        
        bulletHandler = GameObject.Find("BulletHandler").GetComponentInParent<BulletHandler>();
        bulletHandler.OnBulletExplosion += Shake;
    }

    private void Shake(object sender, BulletHandler.OnBulletExplosionArgs args) {
        Debug.Log("Hello");
        CinemachineBasicMultiChannelPerlin cinemachineBasicMultiChannelPerlin = cinemachineVirtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();

        cinemachineBasicMultiChannelPerlin.m_AmplitudeGain = 5f;
        shakeTime = 3.5f;
    }

    void Update() {
        if (shakeTime > 0) {
            shakeTime -= Time.deltaTime;
            if (shakeTime <= 0f) {
                CinemachineBasicMultiChannelPerlin cinemachineBasicMultiChannelPerlin = cinemachineVirtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();

                cinemachineBasicMultiChannelPerlin.m_AmplitudeGain = 0f;
            }
        }
    }
}
