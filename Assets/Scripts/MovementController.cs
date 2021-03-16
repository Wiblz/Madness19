using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementController : MonoBehaviour {    
    public float speed = 3.0f;
    public float jumpForce = 5.0f;

    private AnimationController animationController;
    public UIController uiController;

    [SerializeField]
    public Weapon[] weapons;
    public Weapon currentWeapon;
    public event EventHandler<OnWeaponChangedArgs> OnWeaponChanged;

    public class OnWeaponChangedArgs : EventArgs {
        public Weapon newWeapon;
    }

    Vector2 movement = new Vector2();
    Rigidbody2D rb2D;
    Collider2D boxCollider;

    public LayerMask solidSurface;
    public GameObject crosshairPrefab;
    public Transform crosshair;

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
        uiController = GameObject.Find("UI").GetComponent<UIController>();

        rb2D = GetComponent<Rigidbody2D>();
        boxCollider = GetComponent<BoxCollider2D>();
        crosshair = Instantiate(crosshairPrefab, Vector2.zero, Quaternion.identity, gameObject.transform).transform;

        currentWeapon = weapons[0];
        uiController.StartDashCooldown();
    }

    void Update() {
        movement.x = Input.GetAxisRaw("Horizontal");
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
            }
        }

        // if (isGrounded) {
        //     if (isFalling) {
        //         isFalling = false;
        //         // Check damage
        //         float delta = fallStart - transform.position.y;
        //         Debug.Log($"Fall distance: {delta}");
        //     }
        // } else {
        //     if (!isFalling && rb2D.velocity.y < 0) {
        //         isFalling = true;
        //         fallStart = transform.position.y;
        //     } else if (isFalling && rb2D.velocity.y >= 0) {
        //         isFalling = false;
        //     }
        // }

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

        if (Input.GetKeyDown("1")) {
            ChangeWeapon(0);
        } else if (Input.GetKeyDown("2")) {
            ChangeWeapon(1);
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
            MoveCharacter();
        }
    }

    private void ChangeWeapon(int slot) {
        if (currentWeapon != weapons[slot]) {
            currentWeapon = weapons[slot];
            OnWeaponChanged?.Invoke(this, new OnWeaponChangedArgs{ newWeapon = currentWeapon });   
        }
    }

    private void Jump() {
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
            dashingDirection = crosshair.position.x - transform.position.x  > 0 ? Vector2.right : Vector2.left;
        } else {
            dashingDirection = crosshair.position - transform.position;
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
        // dashAvailable = true;
    }

    private void UpdateState() {
        if (movement.x != 0) {
            animationController.Walk();
        } else {
            animationController.Idle();
        }
    }

    void OnDrawGizmos() {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.down * 0.6f);
        if (boxCollider != null) {
            Gizmos.DrawLine(transform.position - Vector3.right * boxCollider.bounds.size.x / 2f, (transform.position - Vector3.right * boxCollider.bounds.size.x / 2f) + Vector3.down * 0.6f);
            Gizmos.DrawLine(transform.position + Vector3.right * boxCollider.bounds.size.x / 2f, (transform.position + Vector3.right * boxCollider.bounds.size.x / 2f) + Vector3.down * 0.6f);
        }        
    }
}
