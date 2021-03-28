using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementController : MonoBehaviour {    
    public float speed = 3.0f;
    public float jumpForce = 5.0f;

    public bool debugMode = false;

    private AnimationController animationController;
    public UIController uiController;
    private PlayerModelController playerModelController;

    Vector2 movement = new Vector2();
    Rigidbody2D rb2D;
    Collider2D boxCollider;

    public LayerMask solidSurface;
    // public GameObject crosshairPrefab;
    public GameObject crosshair;

    bool isControllable = true;
    
    [Header("Jumping")]
    public bool isGrounded = false;
    public int maxJumps = 2; 
    float jumpDelay = 0f;
    int jumpsAvailable = 0;

    public bool isFalling = false;
    float fallStart;

    [Header("Dashing")]
    public bool isDashing = false;
    public float dashForce = 10f;
    public float dashDuration = 0.1f;
    Vector2 dashingDirection;
    public bool dashAvailable = false;
    IEnumerator dashCoroutine;

    void Start() {
        animationController = GetComponent<AnimationController>();
        // uiController = GameObject.Find("UI").GetComponent<UIController>();
        playerModelController = GetComponent<PlayerModelController>();
        playerModelController.OnPlayerDeath += HandlePlayerDeath;

        rb2D = GetComponent<Rigidbody2D>();
        if (debugMode) {
            rb2D.gravityScale = 0;
        }

        boxCollider = GetComponent<BoxCollider2D>();
        // crosshair = Instantiate(crosshairPrefab, Vector2.zero, Quaternion.identity, gameObject.transform);
    }

    void Update() {
        movement.x = Input.GetAxisRaw("Horizontal");

        if (debugMode) {
            movement.y = Input.GetAxisRaw("Vertical");
        }
        movement.Normalize();

        isGrounded = GroundCheck();
        if (Time.time > jumpDelay && isGrounded) {
            Ground();
        }

        if (isFalling) {
            float delta = fallStart - transform.position.y;

            // just landed, deal damage
            if (isGrounded) {
                isFalling = false;
                //check damage
                // Debug.Log($"Fall distance: {delta}");
            } else {
                if (delta > 40) {
                    // deal void damage
                }
                // just double jumped/dashed, reset the fall
                if (rb2D.velocity.y >= 0) {
                    isFalling = false;
                }
            }
        } else {
            // start fall
            if (rb2D.velocity.y < 0) {
                isFalling = true;
                fallStart = transform.position.y;
                // Debug.Log($"Fall started at {transform.position}");
            }
        }

        if (isControllable) {
            if (Input.GetButtonDown("Jump")) {
                if (jumpsAvailable > 0) {
                    jumpDelay = Time.time + 0.2f;
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
        }

        // DEBUGGING
        if (Input.GetKeyDown("p")) {
            Debug.Log($"Current position: {transform.position}");
        }

        UpdateState();
    }

    bool GroundCheck() {
        return Physics2D.Raycast(transform.position, Vector2.down, 0.6f, solidSurface) ||
               Physics2D.Raycast(transform.position - Vector3.right * boxCollider.bounds.size.x / 2f, Vector2.down, 0.6f, solidSurface) ||
               Physics2D.Raycast(transform.position + Vector3.right * boxCollider.bounds.size.x / 2f, Vector2.down, 0.6f, solidSurface);
    }

    void FixedUpdate() {
        if (isDashing) {
            rb2D.AddForce(dashingDirection * dashForce, ForceMode2D.Impulse);
        } else {
            if (debugMode) {
                DebugMoveCharacter();
            } else {
                MoveCharacter();
            }
        }
    }

    private void Jump() {
        // Debug.Log($"Jump start {transform.position}");
        rb2D.velocity = new Vector2(rb2D.velocity.x, 0);
        rb2D.AddForce(Vector2.up * 30f, ForceMode2D.Impulse);
    }

    private IEnumerator Dash() {
        var watch = System.Diagnostics.Stopwatch.StartNew();

        isDashing = true;
        dashAvailable = false;
        uiController.StartDashCooldown();
        isControllable = false;
        rb2D.gravityScale = 0f;
        rb2D.velocity = Vector2.zero;
        
        if (isGrounded) {
            dashingDirection = crosshair.transform.position.x - transform.position.x  > 0 ? Vector2.right : Vector2.left;
        } else {
            dashingDirection = crosshair.transform.position - transform.position;
        }

        dashingDirection.Normalize();

        yield return new WaitForSeconds(dashDuration);

        isDashing = false;
        isControllable = true;
        rb2D.gravityScale = 17.5f;
        rb2D.velocity = Vector2.zero;

        watch.Stop();
        Debug.Log($"Dash lasted for {watch.ElapsedMilliseconds} milliseconds.");
    }

    private void DebugMoveCharacter() {
        if (movement.x != 0) {
            if (animationController.facingRight != movement.x > 0) {
                animationController.Flip();
            }
        }
        rb2D.velocity = new Vector2(movement.x * speed, movement.y * speed);
    }

    private void MoveCharacter() {
        if (movement.x != 0) {
            if (animationController.facingRight != movement.x > 0) {
                animationController.Flip();
            }
        }
        rb2D.velocity = new Vector2(movement.x * speed, rb2D.velocity.y);
    }

    // Resets multijump and dash 
    private void Ground() {
        jumpsAvailable = maxJumps;
    }

    private void UpdateState() {
        if (movement.x != 0) {
            animationController.Walk();
        } else {
            animationController.Idle();
        }
    }

    void OnDisable() {
        crosshair.SetActive(false);
    }

    void OnEnable() {
        if (crosshair) crosshair.SetActive(true);
    }

    void HandlePlayerDeath(object sender, EventArgs args) {
        enabled = false;
    }

    void OnDrawGizmos() {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.down * 0.6f);
        if (boxCollider != null) {
            Gizmos.DrawLine(transform.position - Vector3.right * boxCollider.bounds.size.x / 2f, (transform.position - Vector3.right * boxCollider.bounds.size.x / 2f) + Vector3.down * 0.6f);
            Gizmos.DrawLine(transform.position + Vector3.right * boxCollider.bounds.size.x / 2f, (transform.position + Vector3.right * boxCollider.bounds.size.x / 2f) + Vector3.down * 0.6f);
        }

        // Debug.Log($"{crosshair.transform.position} {transform.position}");
        // if (crosshair) {
        //     Vector3 crosshairDir = crosshair.transform.position - transform.position;
        //     Gizmos.DrawLine(transform.position, transform.position + crosshairDir);
        //     Gizmos.DrawLine(transform.position, transform.position + VectorByAngle(crosshairDir, 45 * Mathf.Deg2Rad));
        //     Gizmos.DrawLine(transform.position, transform.position + VectorByAngle(crosshairDir, -45 * Mathf.Deg2Rad));
        // }
    }
}
