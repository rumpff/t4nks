using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "newWeaponInfo", menuName = "t4nk/Weapon Info")]
public class WeaponInfo : ScriptableObject
{
    public string Name;
    public Mesh Mesh;
    public float InitialForce;
    public float AccuracyPercentage;
    public float Mass;
}
