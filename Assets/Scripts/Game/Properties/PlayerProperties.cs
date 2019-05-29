using System;
using System.Collections.Generic;
using UnityEngine;
using XboxCtrlrInput;

[CreateAssetMenu(fileName = "newPlayerProperties", menuName = "t4nk/Player Properties")]
public class PlayerProperties : ScriptableObject
{
    public string Name;
    public float Score;
    public TankProperties Tank;
    public XboxController Controller;
    public Dictionary<StatTypes, float> Stats;

    public PlayerProperties()
    {
        Name = string.Empty;
        Tank = new TankProperties();
        Controller = XboxController.Any;
        Stats = new Dictionary<StatTypes, float>();

        foreach (StatTypes foo in Enum.GetValues(typeof(StatTypes)))
        {
            Stats[foo] = 0.0f;
        }
    }
}