using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerModelController : MonoBehaviour {
    public Creature creature;
    Weapon[] weapons;
    public Weapon currentWeapon;

    public UIController uiController;
    BulletHandler bulletHandler;
    public event EventHandler OnPlayerDeath;
    public event EventHandler OnWeaponChanged;

    IEnumerator regenerationCoroutine;

    void Start() {
        Debug.Log($"Player Model Controller Start");

        creature = new Creature(1500);
        weapons = new Weapon[] { new Weapon(), new EnderGun(), new Trident() };
        
        uiController.creature = creature;
        uiController.StartDashCooldown();

        ChangeWeapon(0);

        regenerationCoroutine = Regenerate();
        StartCoroutine(regenerationCoroutine);        
    }

    void Update() {
        if (Input.GetKeyDown("1")) {
            ChangeWeapon(0);
        } else if (Input.GetKeyDown("2")) {
            ChangeWeapon(1);
        } else if (Input.GetKeyDown("3")) {
            ChangeWeapon(2);
        } 
    }

    void ChangeWeapon(int index) {
        Debug.Log("Weapon changed called.");
        currentWeapon = weapons[index];
        OnWeaponChanged?.Invoke(this, EventArgs.Empty);
    }

    public void Hit(float dmg) {
        creature.HP -= dmg;
        if (creature.HP <= 0) {
            OnPlayerDeath?.Invoke(this, EventArgs.Empty);
        }

        uiController.StartDamageEffect();
    }

    private IEnumerator Regenerate() {
        while (true) {
            creature.HP = Mathf.Min(creature.HP + creature.regeneration / 10f, creature.maxHp);
            yield return new WaitForSeconds(0.1f);
        }
    }
}
