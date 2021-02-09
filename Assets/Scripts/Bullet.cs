using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    // Start is called before the first frame update
    void Start() {
        Destroy(gameObject, 5f);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnCollisionEnter(Collision c) {
        Debug.Log("Name of the object: " + c.gameObject.name);
    }

    void OnTriggerEnter(Collider other) {
        Debug.Log("Name of the object: " + other.gameObject.name);
    }
}
