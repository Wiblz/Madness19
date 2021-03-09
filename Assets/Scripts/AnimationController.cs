using UnityEngine;


public class AnimationController : MonoBehaviour {    
    Animator animator;
    string animationState = "AnimationState";
    public bool facingRight = true;

    enum CharStates {
        walk = 1,
        idle = 2
    }

    void Start() {
        animator = GetComponent<Animator>();
    }

    public void Flip() {
        facingRight = !facingRight;

        Vector3 ls = transform.localScale;
        ls.x *= -1f;
        transform.localScale = ls;
    }

    public void Walk() {
        animator.SetInteger(animationState, (int)CharStates.walk);
    }

    public void Idle() {
        animator.SetInteger(animationState, (int)CharStates.idle);
    }
}
