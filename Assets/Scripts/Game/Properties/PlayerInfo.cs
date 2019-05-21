using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "newPlayerInfo", menuName = "t4nk/Player Info")]
public class PlayerInfo : ScriptableObject
{
    public string Name;
    public TankProperties Tank;
    public WeaponProperties DefaultWeapon;
}
