using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameController : MonoBehaviour {
    public Creature creature;

    GameObject healthBar;
    public Slider slider;
    Text regen;
    IEnumerator regenerationCoroutine;

    void Start() {
        creature = new Creature(1500);
        
        healthBar = GameObject.Find("Health Bar"); 
        slider = healthBar.GetComponent<Slider>();
        slider.maxValue = creature.maxHp;
        regen = healthBar.transform.Find("Health Regen").GetComponent<Text>();

        regenerationCoroutine = Regenerate();
        StartCoroutine(regenerationCoroutine);        
    }

    void Update() {
        slider.value = creature.HP;
        regen.text = $"+{creature.regeneration}";
    }

    private IEnumerator Regenerate() {
        while (true) {
            creature.HP = Mathf.Min(creature.HP + creature.regeneration / 10f, creature.maxHp);
            Debug.Log(creature.HP);
            yield return new WaitForSeconds(0.1f);
        }
    }
}
