using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrosshairController : MonoBehaviour {
    public BulletHandler bulletHandler;
    public Weapon weapon;

    PlayerModelController playerModelController;

    Transform firePoint;
    Vector2 firePointPosition;

    Vector2 position = new Vector2();

    void Start() {
        firePoint = transform.parent.GetChild(0);
        bulletHandler = GameObject.Find("BulletHandler").GetComponentInParent<BulletHandler>();

        Cursor.visible = false;
    }

    void Update() {
        if (weapon != null) {
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
                weapon.Shoot(firePoint.position, aimingDirection);
                // bulletHandler.SpawnProjectile(weapon, firePoint.position, aimingDirection);
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

    void UpdateWeapon(object sender, EventArgs args) {
        weapon = playerModelController.currentWeapon;
    }
}
