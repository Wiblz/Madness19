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
    Text currentHealth;
    Image image;
    Image damageEffect;

    IEnumerator dashCooldownCoroutine;
    IEnumerator damageEffectCoroutine;

    void Start() {
        healthBar = GameObject.Find("Health Bar"); 
        slider = healthBar.GetComponent<Slider>();
        slider.maxValue = creature.maxHp;
        regen = healthBar.transform.Find("Health Regen").GetComponent<Text>();
        currentHealth = healthBar.transform.Find("Current Health").GetComponent<Text>();
        image = GameObject.Find("Cooldown Fill").GetComponent<Image>();
        damageEffect = GameObject.Find("Damage").GetComponent<Image>();
    }

    void Update() {
        slider.value = creature.HP;
        regen.text = $"+{creature.regeneration}";
        currentHealth.text = $"{(int)creature.HP} / {creature.maxHp}";
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

    public void StartDamageEffect() {
        if (damageEffectCoroutine != null) {
            StopCoroutine(damageEffectCoroutine);
        }

        damageEffectCoroutine = DamageEffect();
        StartCoroutine(damageEffectCoroutine);
    }

    IEnumerator DamageEffect() {
        damageEffect.color = new Color(1f, 0f, 0f, 0.5f);
        while (damageEffect.color.a > 0f) {
            Color fixedColor = damageEffect.color;
            fixedColor.a -= 0.03f;
            damageEffect.color = fixedColor;

            yield return new WaitForSeconds(0.1f);
        }

        // damageEffect.CrossFadeAlpha(0.5f, 1f, true);
        // damageEffect.CrossFadeAlpha(0f, 1f, true);

        // damageEffect.CrossFadeAlpha(0.5f, 1f, true);
        // damageEffect.CrossFadeAlpha(0.0f, 2f, true);

        yield return null;
        // damageEffect.color = new Color(1f, 0f, 0f, 0.0f);
    }
}
