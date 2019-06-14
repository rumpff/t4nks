using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameRules
{
    public float TimeLimit;
    public float ScoreLimit;
    public float RespawnDelay;
    public Dictionary<StatTypes, float> ScoreValues;

    public float LonghitThreshold;
    public float ScorePerDamageMultiplier;

    public float HealthPickupHealthAmount;
    public float HealthPickupRespawnTime;

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
        LonghitThreshold = 150.0f;
        ScorePerDamageMultiplier = 1.5f;

        HealthPickupHealthAmount = 100;
        HealthPickupRespawnTime = 3.0f;

        ScoreValues = new Dictionary<StatTypes, float>();
        foreach (StatTypes sType in Enum.GetValues(typeof(StatTypes)))
        {
            ScoreValues[sType] = 0.0f;
        }

        ScoreValues[StatTypes.Kill] = 3000.0f;
        ScoreValues[StatTypes.LongHit] = 400.0f;
        ScoreValues[StatTypes.SelfKill] = -500.0f;
    }
}
