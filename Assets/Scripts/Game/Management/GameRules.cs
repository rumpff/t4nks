using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameRules
{
    public float TimeLimit;
    public int ScoreLimit;
    public float RespawnDelay;
    public Dictionary<StatTypes, int> ScoreValues;

    public float HealthPickupHealthAmount;
    public float HealthPickupRespawnTime;

    public float LonghitThreshold;

    public GameRules()
    {
        Default();
    }

    public void Default()
    {
        // Hardcode default values
        TimeLimit = 600.0f;
        ScoreLimit = 20;
        RespawnDelay = 3.0f;

        HealthPickupHealthAmount = 100;
        HealthPickupRespawnTime = 3.0f;

        LonghitThreshold = 350.0f;

        ScoreValues = new Dictionary<StatTypes, int>();
        foreach (StatTypes sType in Enum.GetValues(typeof(StatTypes)))
        {
            ScoreValues[sType] = 0;
        }

        ScoreValues[StatTypes.Kill] = 1;
        ScoreValues[StatTypes.SelfKill] = -1;
    }
}
