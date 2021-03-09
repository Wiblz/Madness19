public class Creature {
    public float maxHp;
    public float HP;
    public float armor;

    public float speed;
    public float jumpForce;

    public Creature(float _maxHp) {
        maxHp = _maxHp;
        HP = maxHp;
        armor = 0f;
    }
}
