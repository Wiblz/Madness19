using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrosshairController : MonoBehaviour
{
    // Start is called before the first frame update

    public Transform firePoint;
    Vector2 firePointPosition;
    
    Vector2 position = new Vector2();
    public float maxDistance = 3.5f;

    void Start()
    {
        Cursor.visible = false;
    }

    // Update is called once per frame
    void Update()
    {
        position = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        firePointPosition = new Vector2(firePoint.position.x, firePoint.position.y);
        float pointerDistance = System.Math.Abs(Vector2.Distance(position, firePointPosition));

        // Debug.Log(firePointPosition);

        if (pointerDistance <= maxDistance) {
            transform.position = position;
        } else {
            Vector2 temp = position - firePointPosition;
            temp.Normalize();
            transform.position = temp * maxDistance + firePointPosition;
        }
    }
}
