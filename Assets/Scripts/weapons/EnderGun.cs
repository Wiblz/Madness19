using UnityEngine;

public class EnderGun : Weapon {
    float teleportDamage = 150f;
    
    public EnderGun() {}

    public override void OnBulletExplosion(Vector2 position) {
        bulletHandler.Teleport(position, teleportDamage);
    }
}
