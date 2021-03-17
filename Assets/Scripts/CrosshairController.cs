using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrosshairController : MonoBehaviour {
    public BulletHandler bulletHandler;
    Weapon weapon;

    Transform firePoint;
    Vector2 firePointPosition;

    bool a = false;
    
    Vector2 position = new Vector2();

    void Awake() {
        weapon = GetComponentInParent<MovementController>().currentWeapon;
        firePoint = transform.parent.GetChild(0);
        GetComponentInParent<MovementController>().OnWeaponChanged += ChangeWeapon;
        bulletHandler = GameObject.Find("BulletHandler").GetComponentInParent<BulletHandler>();

        Cursor.visible = false;
    }

    private void ChangeWeapon(object sender, MovementController.OnWeaponChangedArgs args) {
        weapon = args.newWeapon;
    }

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
            bulletHandler.Spawn(weapon, firePoint.position, aimingDirection);
            // DEBUGGING
            // if (!a) {
            //     bulletHandler.Explode(new Vector2(-2.0f, -21.5f), 1.5f, 1000f, 50f);
            //     bulletHandler.Explode(new Vector2(-1.4f, -22.1f), 1.5f, 1000f, 50f);
            //     bulletHandler.Explode(new Vector2(-1.1f, -21.4f), 1.5f, 1000f, 50f);
            //     bulletHandler.Explode(new Vector2(-0.5f, -21.0f), 1.5f, 1000f, 50f);
            //     a = true;
            // } else {
            //     bulletHandler.Explode(new Vector2(0.5f, -20.3f), 1.5f, 1000f, 50f);
            // }

            // GameObject blt = Instantiate(bullet, firePoint.position, Quaternion.identity);
            // Physics2D.IgnoreCollision(blt.GetComponent<CircleCollider2D>(), player.GetComponent<Collider2D>());
            // blt.GetComponent<Rigidbody2D>().AddForce(aimingDirection * weapon.power, ForceMode2D.Impulse);
        }
    }
}
