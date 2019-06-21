using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "newTankProperties", menuName = "t4nk/Tank Properties")]
public class TankProperties : ScriptableObject
{
    public string Name;
    public float MaxHealth;
    public float MaxTorque;
    public float AccelerateRate;
    public float MaxSteer;
    public float BrakeTorque;
    public float JumpForce;
    public int AirJumpAmount;
    public float AirControl;

    public WeaponProperties DefaultWeapon;
    public GameObject ExplosionPrefab;
    public ExplosionProperties DestroyExplosionProperties;
}
