using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour {
    bool ignore = true;

    // Start is called before the first frame update
    void Start() {
        Destroy(gameObject, 5f);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnTriggerEnter2D(Collider2D other) {
        Debug.Log("Name of the object: " + other.gameObject.name);
        if (ignore) {
            ignore = false;
        } else {
            Destroy(gameObject);
        }
    }
}
