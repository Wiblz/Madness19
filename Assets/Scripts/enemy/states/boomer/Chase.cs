using UnityEngine;

public class Chase : BoomerState {
    public GameObject target;
    public Chase(BoomerController _controller, GameObject _target) : base(_controller) {
        target = _target;
    }

    private bool IsTargetVisible(Vector2 direction) {
        var hit = Physics2D.Raycast(controller.transform.position, direction);

        Debug.Log(hit);
        return hit.collider != null && hit.collider.tag == "Player";
    }

    public override void Update() {
        float distance = Vector2.Distance(target.transform.position, controller.transform.position);
        Vector2 direction = target.transform.position - controller.transform.position;

        if (distance >= 10f) {
            controller.SetState(new Idle(controller));
        } else if (distance < 5f && IsTargetVisible(direction)) {
            Debug.Log("BAM!!");
        } else {
            float l = Mathf.InverseLerp(5f, 10f, distance);
            float speedMultiplier = controller.distanceChaseSpeedCurve.Evaluate(l);
        
            direction.Normalize();

            // Debug.Log($"Speed multiplier {speedMultiplier}");
            // Debug.Log($"{l} {distance} {speedMultiplier}");
            controller.rb2D.velocity = direction * 5f * speedMultiplier;
        }
    }
}
