using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour {
    BulletHandler bulletHandler;
    Weapon weapon;

    void Start() {
        bulletHandler = gameObject.GetComponentInParent<BulletHandler>();
        Destroy(gameObject, 5f);
    }

    void Update() {
        
    }

    public void SetWeapon(Weapon _weapon) {
        weapon = _weapon;
    }

    void OnCollisionEnter2D(Collision2D other) {
        Debug.Log(other.GetContact(0).point);
        Destroy(gameObject);
        weapon.OnBulletExplosion(other.GetContact(0).point);
        // bulletHandler.Explode(other.GetContact(0).point, 1.5f, 1000f, 50f);
    }
}
