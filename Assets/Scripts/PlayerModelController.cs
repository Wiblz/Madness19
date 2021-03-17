using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerModelController : MonoBehaviour {
    public Creature creature;

    UIController uiController;
    MovementController movementController;
    public event EventHandler OnPlayerDeath;

    IEnumerator regenerationCoroutine;

    void Start() {
        creature = new Creature(1500);
        
        uiController = GameObject.Find("UI").GetComponent<UIController>();
        uiController.creature = creature;
        uiController.enabled = true;

        movementController = GetComponent<MovementController>();
        uiController.movementController = movementController;
        movementController.enabled = true;

        regenerationCoroutine = Regenerate();
        StartCoroutine(regenerationCoroutine);        
    }

    void Update() {

    }

    public void Hit(float dmg) {
        creature.HP -= dmg;
        if (creature.HP <= 0) {
            movementController.enabled = false;
            OnPlayerDeath?.Invoke(this, EventArgs.Empty);
        }
    }

    private IEnumerator Regenerate() {
        while (true) {
            creature.HP = Mathf.Min(creature.HP + creature.regeneration / 10f, creature.maxHp);
            yield return new WaitForSeconds(0.1f);
        }
    }
}
