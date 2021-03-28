using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class GameController : MonoBehaviour {
    public PostProcessProfile profile;
    // public GameObject UIPrefab;
    public GameObject UIContainer;
    Spawner spawner;
    GameObject player;
    public PlayerModelController playerModelController;
    public CrosshairController crosshairController;

    ColorGrading colorGrading;

    void Start() {
        spawner = GetComponent<Spawner>();
        spawner.Init();
        colorGrading = profile.GetSetting<ColorGrading>();
        colorGrading.saturation.value = 0f;

        // UIContainer = Instantiate(UIPrefab, Vector2.zero, Quaternion.identity);
        // UIContainer.transform.name = "UI";

        player = spawner.SpawnPlayer();
        // playerModelController = player.GetComponent<PlayerModelController>();
        // playerModelController.uiController = UIContainer.GetComponent<UIController>();
        playerModelController.OnPlayerDeath += HandlePlayerDeath;
        playerModelController.OnWeaponChanged += ChangeWeapon;

        // crosshairController = GameObject.Find("Crosshair").GetComponent<CrosshairController>(); 
        UIContainer.SetActive(true);
    }

    void HandlePlayerDeath(object sender, EventArgs args) {
        StartCoroutine(FadeScreen());
        UIContainer.SetActive(false);
    }

    void ChangeWeapon(object sender, EventArgs args) {
        crosshairController.weapon = playerModelController.currentWeapon;
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
