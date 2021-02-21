using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletHandler : MonoBehaviour {
    public GameObject player;
    public GameObject bullet;

    public event EventHandler<OnBulletExplosionArgs> OnBulletExplosion;

    public class OnBulletExplosionArgs : EventArgs {
        public Vector2 position;
        public float radius;
        public float power;
    }

    // Start is called before the first frame update
    void Start() {
        player = GameObject.Find("PlayerObject");
    }

    // Update is called once per frame
    void Update() {
        
    }

    public void Spawn(Weapon weapon, Vector3 position, Vector3 direction) {
        GameObject blt = Instantiate(bullet, position, Quaternion.identity, gameObject.transform);
        Physics2D.IgnoreCollision(blt.GetComponent<CircleCollider2D>(), player.GetComponent<Collider2D>());
        blt.GetComponent<Rigidbody2D>().AddForce(direction * weapon.power, ForceMode2D.Impulse);
    }

    public void Explode(Vector2 position, float radius, float power) {
        OnBulletExplosion?.Invoke(this, new OnBulletExplosionArgs{ position = position, radius = radius, power = power });
    }
}
