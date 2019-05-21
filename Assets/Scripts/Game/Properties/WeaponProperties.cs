using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "newWeaponProperties", menuName = "t4nk/Weapon Properties")]
public class WeaponProperties : ScriptableObject
{
    public string Name;
    public Mesh Mesh;
    public float InitialForce;
    public float TankForce;
    public float AccuracyPercentage;
    public float Mass;
    public float Cooldown;
    public float CameraImpact;
    public float BurnoutTime;
	public Explosion Explosion;
    public ExplosionProperties ExplosionProperties;
}
