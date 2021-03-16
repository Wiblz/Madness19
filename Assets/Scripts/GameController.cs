using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameController : MonoBehaviour {
    public Creature creature;

    UIController uiController;
    MovementController movementController;

    GameObject healthBar;
    public Slider slider;
    Text regen;
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

    private IEnumerator Regenerate() {
        while (true) {
            creature.HP = Mathf.Min(creature.HP + creature.regeneration / 10f, creature.maxHp);
            yield return new WaitForSeconds(0.1f);
        }
    }
}
