using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementController : MonoBehaviour {    
    public float speed = 3.0f;
    public float jumpForce = 5.0f;

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
    Animator animator;
    string animationState = "AnimationState";

    public LayerMask solidSurface;
    public Transform crosshair;

    public bool facingRight = true;
    bool isControllable = true;
    
    [Header("Jumping")]
    public bool isGrounded = false;
    public int maxJumps = 2; 
    float jumpDelay = 0f;
    int jumpsAvailable = 0;

    [Header("Dashing")]
    public bool isDashing = false;
    public float dashForce = 10f;
    public float dashDuration = 0.1f;
    Vector2 dashingDirection;
    bool dashAvailable = false;
    IEnumerator dashCoroutine;

    enum CharStates {
        walk = 1,
        idle = 2
    }

    // Start is called before the first frame update
    void Start() {
        rb2D = GetComponent<Rigidbody2D>();
        boxCollider = GetComponent<BoxCollider2D>();
        animator = GetComponent<Animator>();

        currentWeapon = weapons[0];
        transform.position = new Vector3(35.5f, 8.1f, 0.0f);
    }

    // Update is called once per frame
    void Update() {
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.Normalize();

        isGrounded = GroundCheck();
        if (Time.time > jumpDelay && isGrounded) {
            Ground();
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

        if (Input.GetKeyDown("1")) {
            ChangeWeapon(0);
        } else if (Input.GetKeyDown("2")) {
            ChangeWeapon(1);
        }

        // DEBUGGING
        if (Input.GetKeyDown("p")) {
            Debug.Log($"{transform.position}");
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
        isDashing = true;
        dashAvailable = false;
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
        // Debug.Log("Setting v to 0");
        rb2D.velocity = Vector2.zero;
    }

    private void MoveCharacter() {
        if (movement.x != 0) {
            if (facingRight != movement.x > 0) {
                facingRight = !facingRight;

                Vector3 ls = transform.localScale;
                ls.x *= -1f;
                transform.localScale = ls;
            }

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
        if (boxCollider != null) {
            Gizmos.DrawLine(transform.position - Vector3.right * boxCollider.bounds.size.x / 2f, (transform.position - Vector3.right * boxCollider.bounds.size.x / 2f) + Vector3.down * 0.6f);
            Gizmos.DrawLine(transform.position + Vector3.right * boxCollider.bounds.size.x / 2f, (transform.position + Vector3.right * boxCollider.bounds.size.x / 2f) + Vector3.down * 0.6f);
        }        
    }
}
