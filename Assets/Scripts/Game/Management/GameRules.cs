using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameRules
{
    public float TimeLimit;
    public float ScoreLimit;
    public float RespawnDelay;

    public GameRules()
    {
        Default();
    }

    public void Default()
    {
        // Hardcode default values
        TimeLimit = 600.0f;
        ScoreLimit = 10000.0f;
        RespawnDelay = 3.0f;
    }
}
