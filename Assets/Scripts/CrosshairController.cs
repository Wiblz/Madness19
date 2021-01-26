using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrosshairController : MonoBehaviour {
    // Start is called before the first frame update
    public GameObject bullet;
    public GameObject player;
    Weapon weapon;

    public Transform firePoint;
    Vector2 firePointPosition;
    
    Vector2 position = new Vector2();

    void Start() {
        player = GameObject.Find("PlayerObject");
        weapon = player.GetComponent<MovementController>().currentWeapon;
        player.GetComponent<MovementController>().OnWeaponChanged += ChangeWeapon;
        Cursor.visible = false;
    }

    private void ChangeWeapon(object sender, MovementController.OnWeaponChangedArgs args) {
        weapon = args.newWeapon;
    }

    // Update is called once per frame
    void Update() {
        position = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        firePointPosition = new Vector2(firePoint.position.x, firePoint.position.y);
        float pointerDistance = System.Math.Abs(Vector2.Distance(position, firePointPosition));

        Vector2 aimingDirection = position - firePointPosition;
        aimingDirection.Normalize();

        if (pointerDistance <= weapon.aimingDistance) {
            transform.position = position;
        } else {
            transform.position = aimingDirection * weapon.aimingDistance + firePointPosition;
        }

        if (Input.GetButtonDown("Fire1")) {
            Rigidbody2D rb = Instantiate(bullet, firePoint.position, Quaternion.identity).GetComponent<Rigidbody2D>();
            rb.AddForce(aimingDirection * weapon.power, ForceMode2D.Impulse);
        }
    }
}
