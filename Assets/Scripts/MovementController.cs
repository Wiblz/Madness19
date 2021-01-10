using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementController : MonoBehaviour
{
    public float speed = 3.0f;
    public float jumpForce = 5.0f;

    Vector2 movement = new Vector2();
    Rigidbody2D rb2D;
    Animator animator;
    string animationState = "AnimationState";

    public LayerMask solidSurface;
    public Transform crosshair;

    public bool facingRight = true;
    public bool isGrounded = false;
    public int maxJumps = 2; 
    public bool dashAvailable = false;
    IEnumerator dashCoroutine;
    int jumpsAvailable = 0;

    enum CharStates {
        walk = 1,
        idle = 2
    }

    // Start is called before the first frame update
    void Start()
    {
        rb2D = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.Normalize();

        isGrounded = Physics2D.Raycast(transform.position, Vector2.down, 0.6f, solidSurface);
        if (isGrounded) {
            Ground();
        }

        if (Input.GetButtonDown("Jump")) {
            if (jumpsAvailable > 0) {
                jumpsAvailable--;
                Jump();
            }
        } else if (Input.GetKeyDown("left shift") && dashAvailable) {
            if (dashCoroutine != null) {
                StopCoroutine(dashCoroutine);
            }
            dashCoroutine = Dash();
            StartCoroutine(dashCoroutine);
        }

        UpdateState();
    }

    void FixedUpdate() {
        MoveCharacter();
    }

    private void Jump() {
        rb2D.velocity = new Vector2(rb2D.velocity.x, 0);
        rb2D.AddForce(Vector2.up * 30f, ForceMode2D.Impulse); 
    }

    private IEnumerator Dash() {
        dashAvailable = false;
        rb2D.gravityScale = 0f;
        rb2D.velocity = Vector2.zero;
        
        // Vector2 direction;
        // if (isGrounded) {
        //     direction = facingRight ? Vector2.right : Vector2.left;
        // } else {
        //     direction = crosshair.position - transform.position;
        // }

        // direction.Normalize();
        // Debug.Log($"{direction.x}, {direction.y}");

        // rb2D.AddForce(direction * 30f, ForceMode2D.Impulse); 
        yield return new WaitForSeconds(0f);
    }

    private void MoveCharacter() {
        if (movement.x != 0) {
            facingRight = movement.x > 0;
            // Vector3 ls = transform.localScale;
            // ls.x *= facingRight ? 1f : -1f;
            // transform.localScale = ls;
        }

        rb2D.velocity = new Vector2(movement.x * speed, rb2D.velocity.y);
    }

    // Resets multijump and dash 
    private void Ground() {
        jumpsAvailable = maxJumps;
        dashAvailable = true;
    }

    private void UpdateState() {
        if (movement.x != 0) {
            animator.SetInteger(animationState, (int)CharStates.walk);
        } else {
            animator.SetInteger(animationState, (int)CharStates.idle);
        }
    }

    void OnDrawGizmos() {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.down * 0.6f);
    }
}
