using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimpleEasing;

[CreateAssetMenu(fileName = "newBarrelAnimation", menuName = "t4nk/Barrel Animation")]
public class BarrelAnimation : ScriptableObject
{
    [Header("Translate X")]
    public bool TXActive;
    [Space(5)]
    public EaseType TXEaseType;
    public float TXStart;
    public float TXChange;
    public float TXDuration;

    [Header("Translate Y")]
    public bool TYActive;
    [Space(5)]
    public EaseType TYEaseType;
    public float TYStart;
    public float TYChange;
    public float TYDuration;

    [Header("Translate Z")]
    public bool TZActive;
    [Space(5)]
    public EaseType TZEaseType;
    public float TZStart;
    public float TZChange;
    public float TZDuration;

    [Header("Rotate X")]
    public bool RXActive;
    [Space(5)]
    public EaseType RXEaseType;
    public float RXStart;
    public float RXChange;
    public float RXDuration;

    [Header("Rotate Y")]
    public bool RYActive;
    [Space(5)]
    public EaseType RYEaseType;
    public float RYStart;
    public float RYChange;
    public float RYDuration;

    [Header("Rotate Z")]
    public bool RZActive;
    [Space(5)]
    public EaseType RZEaseType;
    public float RZStart;
    public float RZChange;
    public float RZDuration;

    [Header("Scale X")]
    public bool SXActive;
    [Space(5)]
    public EaseType SXEaseType;
    public float SXStart;
    public float SXChange;
    public float SXDuration;

    [Header("Scale Y")]
    public bool SYActive;
    [Space(5)]
    public EaseType SYEaseType;
    public float SYStart;
    public float SYChange;
    public float SYDuration;

    [Header("Scale Z")]
    public bool SZActive;
    [Space(5)]
    public EaseType SZEaseType;
    public float SZStart;
    public float SZChange;
    public float SZDuration;
}
