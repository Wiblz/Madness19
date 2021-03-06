using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour {
    bool ignore = true;
    BulletHandler bulletHandler;

    // Start is called before the first frame update
    void Start() {
        bulletHandler = gameObject.GetComponentInParent<BulletHandler>();
        Destroy(gameObject, 5f);
    }

    // Update is called once per frame
    void Update() {
        
    }

    void OnCollisionEnter2D(Collision2D other) {
        Debug.Log(other.GetContact(0).point);
        Destroy(gameObject);
        bulletHandler.Explode(other.GetContact(0).point, 1.5f, 1000f);
    }
}
