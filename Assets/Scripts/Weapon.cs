using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Weapon", menuName = "Weapon")]
public class Weapon : ScriptableObject
{
    public new string name;
    public Sprite icon;    

    public float cooldown;
    public float power;
    public float aimingDistance;
}
