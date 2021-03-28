using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoomerController : StateMachine {    
    public float speed = 3.0f;
    public float visionRadius = 5.0f;
    public AnimationCurve distanceChaseSpeedCurve;
    public GameObject target;
    public Rigidbody2D rb2D;

    private AnimationController animationController;

    Vector2 movement = new Vector2();
    // Rigidbody2D rb2D;
    Collider2D boxCollider;

    void Start() {
        target = GameObject.FindWithTag("Player");
        SetState(new Idle(this));
        animationController = GetComponent<AnimationController>();

        rb2D = GetComponent<Rigidbody2D>();
        boxCollider = GetComponent<BoxCollider2D>();
    }

    void Update() {
        CurrentState.Update();

        UpdateState();
    }

    private void MoveCharacter() {
        if (movement.x != 0) {
            if (animationController.facingRight != movement.x > 0) {
                animationController.Flip();
            }
            rb2D.velocity = new Vector2(movement.x * speed, rb2D.velocity.y);
        }
    }

    private void UpdateState() {
        if (movement.x != 0) {
            animationController.Walk();
        } else {
            animationController.Idle();
        }
    }

    void OnDrawGizmos() {
        Gizmos.DrawWireSphere(transform.position, visionRadius);
    }
}
