public class Creature {
    public float maxHp;
    public float HP;
    public float regeneration;
    public float armor;

    public float speed;
    public float jumpForce;

    public float dashCooldown = 2f;

    public Creature(float _maxHp) {
        maxHp = _maxHp;
        HP = maxHp;
        regeneration = 25f;
        armor = 0f;
    }
}
