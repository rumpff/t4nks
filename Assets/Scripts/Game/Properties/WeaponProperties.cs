using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "newWeaponProperties", menuName = "t4nk/Weapon Properties")]
public class WeaponProperties : ScriptableObject
{
    public string Name;

    [Space(5)]
    public GameObject BarrelPrefab;
    public GameObject ProjectilePrefab;

    [Space(5)]
    public int StartingAmmo;

    [Space(5)]
    public float TankForce;
    public float Cooldown;
    public float CameraImpact;

    [Space(5)]
    public float Mass;
    public float SpreadAngle;
    public float InitialForce;
    public float MaxLifeTime;
    public float BurnoutTime;

    [Space(5)]
	public Explosion Explosion;
    public ExplosionProperties ExplosionProperties;

    [Space(5)]
    public TransformAnimation ReloadAnimation;
}
