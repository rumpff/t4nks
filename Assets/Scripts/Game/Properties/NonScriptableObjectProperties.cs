using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct HitProperties
{
    public bool Valid;

    public int AttackerPlayerIndex;
    public int HitPlayerIndex;
    public float TravelDistance;

    public HitProperties(bool valid)
    {
        Valid = valid;
        AttackerPlayerIndex = -1;
        HitPlayerIndex = -1;
        TravelDistance = -1;
    }
}

public struct GamePlayer
{
    public PlayerProperties Properties;

    public Player Player;
    public PlayerState State;
    public PlayerCamera Camera;
    public PlayerUI UI;
    public int Index;

    public bool AllowedToRespawn;

    public GamePlayer(PlayerProperties properties, int index)
    {
        Properties = properties;
        State = PlayerState.Alive;
        Index = index;
        AllowedToRespawn = false;

        // These values will be assigned once they're created
        Player = null;
        Camera = null;
        UI = null;
    }
}

public enum PlayerState
{
    Alive, Destroyed
};
public enum StatTypes
{
    Jump,
    Death,
    Kill,
    SelfKill,
    DamageDealt,
    DamageRecieved,
    SelfDamage,
    LongHit,
};
