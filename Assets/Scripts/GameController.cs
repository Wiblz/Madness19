using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class GameController : MonoBehaviour {
    public PostProcessProfile profile;
    public GameObject UIContainer;
    Spawner spawner;
    GameObject player;
    PlayerModelController playerModelController;

    ColorGrading colorGrading;

    void Start() {
        spawner = gameObject.GetComponent<Spawner>();
        colorGrading = profile.GetSetting<ColorGrading>();
        colorGrading.saturation.value = 0f;

        player = spawner.SpawnPlayer();
        playerModelController = player.GetComponent<PlayerModelController>();
        playerModelController.OnPlayerDeath += HandlePlayerDeath;
        UIContainer.SetActive(true);
    }

    void HandlePlayerDeath(object sender, EventArgs args) {
        StartCoroutine(FadeScreen());
        UIContainer.SetActive(false);
    }

    IEnumerator FadeScreen() {
        while (colorGrading.saturation.value > -100f) {
            colorGrading.saturation.value -= 5f;

            yield return new WaitForSeconds(0.1f);
        }
    }

    void Update() {
        
    }
}
