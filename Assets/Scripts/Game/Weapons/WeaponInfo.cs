using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "newWeaponInfo", menuName = "t4nk/Weapon Info")]
public class WeaponInfo : ScriptableObject
{
    public string Name;
    public Mesh Mesh;
    public float InitialForce;
    public float TankForce;
    public float AccuracyPercentage;
    public float BaseDamage;
    public float Mass;
    public float Cooldown;
    public float CameraImpact;
	public GameObject[] OnHitSpawnObjects;
}
