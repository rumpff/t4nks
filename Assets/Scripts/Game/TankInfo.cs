using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "newTankInfo", menuName = "t4nk/Tank Info")]
public class TankInfo : ScriptableObject
{
    public string Name;
    public float MaxTorque;
    public float AccelerateRate;
    public float MaxSteer;
    public float BrakeTorque;
}
