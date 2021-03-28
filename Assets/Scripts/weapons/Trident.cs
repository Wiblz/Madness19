using UnityEngine;

public class Trident : Weapon {
    public Trident() {}

    private Vector3 VectorByAngle(Vector3 vector, float radAngle) {
        return new Vector3(vector.x * Mathf.Cos(radAngle) - vector.y * Mathf.Sin(radAngle),
                           vector.x * Mathf.Sin(radAngle) + vector.y * Mathf.Cos(radAngle), 0);
    }

    public override void Shoot(Vector2 position, Vector2 aimingDirection) {
        base.Shoot(position, aimingDirection);
        base.Shoot(position, VectorByAngle(aimingDirection, 45 * Mathf.Deg2Rad));
        base.Shoot(position, VectorByAngle(aimingDirection, -45 * Mathf.Deg2Rad));
        // Gizmos.DrawLine(transform.position, transform.position + VectorByAngle(crosshairDir, 45 * Mathf.Deg2Rad));
        // Gizmos.DrawLine(transform.position, transform.position + VectorByAngle(crosshairDir, -45 * Mathf.Deg2Rad));
    }
}
