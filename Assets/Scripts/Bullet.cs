using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour {
    BulletHandler bulletHandler;

    void Start() {
        bulletHandler = gameObject.GetComponentInParent<BulletHandler>();
        Destroy(gameObject, 5f);
    }

    void Update() {
        
    }

    void OnCollisionEnter2D(Collision2D other) {
        Debug.Log(other.GetContact(0).point);
        Destroy(gameObject);
        bulletHandler.Explode(other.GetContact(0).point, 1.5f, 1000f, 50f);
    }
}
