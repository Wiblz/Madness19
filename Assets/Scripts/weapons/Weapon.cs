using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon {
    protected BulletHandler bulletHandler;

    public string name;
    public Sprite icon;    

    public float cooldown;
    public float power;
    public float knockback;
    public float radius;
    public float aimingDistance;

    public Weapon() {
        aimingDistance = 5f;
        power = 5f;
        bulletHandler = GameObject.Find("BulletHandler").GetComponent<BulletHandler>(); 
    }

    public virtual void Shoot(Vector2 position, Vector2 aimingDirection) {
        bulletHandler.SpawnProjectile(this, position, aimingDirection);
    }

    public virtual void OnBulletExplosion(Vector2 position) {
        bulletHandler.Explode(position, radius, power, knockback);
    }
}
