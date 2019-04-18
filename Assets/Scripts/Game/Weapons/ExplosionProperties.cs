using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "newExplosionProperties", menuName = "t4nk/Explosion Properties")]
public class ExplosionProperties : ScriptableObject
{
    public float BaseDamage;
    public float ExplosionForce;
    public float ExplosionRadius;
    public float MaxLifeTime;
}
