using Cinemachine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletHandler : MonoBehaviour {
    public GameObject player;
    public GameObject bullet;

    CinemachineImpulseSource ImpulseSource;
    ParticleSystem ps;

    public event EventHandler<OnBulletExplosionArgs> OnBulletExplosion;

    public class OnBulletExplosionArgs : EventArgs {
        public Vector2 position;
        public float radius;
        public float power;
        public float knockback;
    }

    void Start() {
        ImpulseSource = GetComponent<CinemachineImpulseSource>();
        ps = GetComponent<ParticleSystem>();

        OnBulletExplosion += ShakeCamera;
        OnBulletExplosion += EmitParticles;
        OnBulletExplosion += ImpactCreatures;
    }

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

    private void ApplyKnockback(Rigidbody2D rb, BulletHandler.OnBulletExplosionArgs args) {
        // rb.AddExplosionForce(args.knockback, args.position, args.radius);

        Debug.Log($"{new Vector2(rb.position.x, rb.position.y)} {args.position}");
        Vector2 dir = (new Vector2(rb.position.x, rb.position.y) - args.position);
        dir.Normalize();

        Debug.Log(dir * args.knockback);
        rb.AddForce(dir * args.knockback, ForceMode2D.Impulse);
    }

    private void DealDamage(GameController gc, Vector2 position, BulletHandler.OnBulletExplosionArgs args) {
        float distance = Vector2.Distance(position, args.position);
        float dmg = Mathf.Max(args.power - (distance / args.radius * args.power), 0);

        gc.creature.HP -= dmg;
    }

    public void ImpactCreatures(object sender, BulletHandler.OnBulletExplosionArgs args) {
        Collider2D[] objects = Physics2D.OverlapCircleAll(args.position, args.radius);

        foreach (Collider2D o in objects) {
            Vector2 position = o.transform.position;
            Rigidbody2D rb = o.GetComponent<Rigidbody2D>();
            if (rb != null) {
                // ApplyKnockback(rb, args);
            }

            GameController gc = o.GetComponent<GameController>();
            if (rb != null) {
                DealDamage(gc, position, args);
            }
        }
    }

    public void EmitParticles(object sender, BulletHandler.OnBulletExplosionArgs args) {
        ParticleSystem.EmitParams emitParams = new ParticleSystem.EmitParams();
        emitParams.position = args.position;
        emitParams.applyShapeToPosition = true;
        ps.Emit(emitParams, 60);
    }
}
