using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "newPlayerGameStats", menuName = "t4nk/Player GameStats")]
public class PlayerGameStats : ScriptableObject
{
    public float Score;
    public int Kills;
    public int Deaths;
}
