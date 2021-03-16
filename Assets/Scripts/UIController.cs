using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour {
    public MovementController movementController;
    public Creature creature;
    GameObject healthBar;
    public Slider slider;
    Text regen;
    Image image;

    IEnumerator dashCooldownCoroutine;

    void Start() {
        healthBar = GameObject.Find("Health Bar"); 
        slider = healthBar.GetComponent<Slider>();
        slider.maxValue = creature.maxHp;
        regen = healthBar.transform.Find("Health Regen").GetComponent<Text>();
        image = GameObject.Find("Cooldown Fill").GetComponent<Image>();
    }

    void Update() {
        slider.value = creature.HP;
        regen.text = $"+{creature.regeneration}";
    }

    public void StartDashCooldown() {
        if (dashCooldownCoroutine != null) {
            StopCoroutine(dashCooldownCoroutine);
        }

        Debug.Log(creature);
        dashCooldownCoroutine = DashCooldown(creature.dashCooldown);
        StartCoroutine(dashCooldownCoroutine);
    }

    IEnumerator DashCooldown(float duration) {
        int steps = (int)(duration * 10);
        for (int i = 1; i <= steps; i++) {
            float fill = (float)(steps - i) / (float)steps;
            image.fillAmount = fill;

            yield return new WaitForSeconds(0.1f);
        }

        movementController.dashAvailable = true;
    }
}
