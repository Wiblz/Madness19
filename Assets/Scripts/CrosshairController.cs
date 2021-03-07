using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrosshairController : MonoBehaviour {
    // Start is called before the first frame update
    public GameObject player;
    public BulletHandler bulletHandler;
    Weapon weapon;

    public Transform firePoint;
    Vector2 firePointPosition;
    
    Vector2 position = new Vector2();

    void Start() {
        player = GameObject.Find("PlayerObject");
        weapon = player.GetComponent<MovementController>().currentWeapon;
        player.GetComponent<MovementController>().OnWeaponChanged += ChangeWeapon;
        bulletHandler = GameObject.Find("BulletHandler").GetComponentInParent<BulletHandler>();

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
            bulletHandler.Spawn(weapon, firePoint.position, aimingDirection);
            // DEBUGGING
            // bulletHandler.Explode(new Vector2(38.2f, 13.5f), 1.5f, 1000f);
            // bulletHandler.Explode(new Vector2(-3.0f, 2.3f), 1.5f, 1000f);
            // bulletHandler.Explode(new Vector2(-1.2f, 3.3f), 1.5f, 1000f);
            // bulletHandler.Explode(new Vector2(-3.0f, 0.3f), 1.5f, 1000f);
            // bulletHandler.Explode(new Vector2(-4.0f, 1.5f), 1.5f, 1000f);
            // bulletHandler.Explode(new Vector2(-2.5f, 1.5f), 1.5f, 1000f);
            // bulletHandler.Explode(new Vector2(-4.0f, -2.5f), 1.5f, 1000f);
            // GameObject blt = Instantiate(bullet, firePoint.position, Quaternion.identity);
            // Physics2D.IgnoreCollision(blt.GetComponent<CircleCollider2D>(), player.GetComponent<Collider2D>());
            // blt.GetComponent<Rigidbody2D>().AddForce(aimingDirection * weapon.power, ForceMode2D.Impulse);
        }
    }
}
