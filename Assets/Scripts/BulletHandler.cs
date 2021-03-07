using Cinemachine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletHandler : MonoBehaviour {
    public GameObject player;
    public GameObject bullet;

    CinemachineImpulseSource ImpulseSource;

    public event EventHandler<OnBulletExplosionArgs> OnBulletExplosion;

    public class OnBulletExplosionArgs : EventArgs {
        public Vector2 position;
        public float radius;
        public float power;
        public float knockback;
    }

    // Start is called before the first frame update
    void Start() {
        ImpulseSource = GetComponent<CinemachineImpulseSource>();
        player = GameObject.Find("PlayerObject");

        OnBulletExplosion += ShakeCamera;
        // OnBulletExplosion += ApplyKnockback;
    }

    // Update is called once per frame
    void Update() {
        
    }

    public void Spawn(Weapon weapon, Vector3 position, Vector3 direction) {
        GameObject blt = Instantiate(bullet, position, Quaternion.identity, gameObject.transform);
        Physics2D.IgnoreCollision(blt.GetComponent<CircleCollider2D>(), player.GetComponent<Collider2D>());
        blt.GetComponent<Rigidbody2D>().AddForce(direction * weapon.power, ForceMode2D.Impulse);
    }

    public void Explode(Vector2 position, float radius, float power, float knockback) {
        OnBulletExplosion?.Invoke(this, new OnBulletExplosionArgs{ position = position, radius = radius, power = power, knockback = knockback });
    }

    public void ShakeCamera(object sender, BulletHandler.OnBulletExplosionArgs args) {
        ImpulseSource.GenerateImpulseAt(args.position, new Vector3(1.0f, 1.0f, 0f));
    }

    public void ApplyKnockback(object sender, BulletHandler.OnBulletExplosionArgs args) {
        Collider2D[] objects = Physics2D.OverlapCircleAll(args.position, args.radius);
        // Debug.Log($"{objects.Length} objects.");

        foreach (Collider2D o in objects) {
            Rigidbody2D rb = o.GetComponent<Rigidbody2D>();
            if (rb != null) {
                // rb.AddExplosionForce(args.knockback, args.position, args.radius);

                Debug.Log($"{new Vector2(o.transform.position.x, o.transform.position.y)} {args.position}");
                Vector2 dir = (new Vector2(o.transform.position.x, o.transform.position.y) - args.position);
                dir.Normalize();

                Debug.Log(dir * args.knockback);
                rb.AddForce(dir * args.knockback, ForceMode2D.Impulse);
                // rb.velocity = dir * args.knockback;
            }
        }
    }
}
