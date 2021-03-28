using UnityEngine;

public class Idle : BoomerState {
    public Idle(BoomerController _controller) : base(_controller) {}

    public override void Update() {
        float distance = Vector2.Distance(controller.target.transform.position, controller.transform.position);
        Debug.Log(distance);

        if (distance <= 5f) {
            controller.SetState(new Chase(controller, controller.target));
        }
    }
}
