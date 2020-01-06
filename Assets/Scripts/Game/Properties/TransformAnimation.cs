using UnityEngine;
using SimpleEasing;

[CreateAssetMenu(fileName = "newBarrelAnimation", menuName = "t4nk/Transform Animation")]
public class TransformAnimation : ScriptableObject
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

    /// <summary>
    /// Returns 0 for inactive values
    /// </summary>
    /// <param name="time"></param>
    /// <returns></returns>
    public Vector3 GetTranslate(float time)
    {
        Vector3 translate = new Vector3
        {
            x = (TXActive) ? Easing.Ease(TXEaseType, time, TXStart, TXChange, TXDuration) : 0,
            y = (TYActive) ? Easing.Ease(TYEaseType, time, TYStart, TYChange, TYDuration) : 0,
            z = (TZActive) ? Easing.Ease(TZEaseType, time, TZStart, TZChange, TZDuration) : 0
        };

        return translate;
    }

    /// <summary>
    /// Returns 0 for inactive values
    /// </summary>
    /// <param name="time"></param>
    /// <returns></returns>
    public Vector3 GetEuler(float time)
    {
        Vector3 euler = new Vector3
        {
            x = (RXActive) ? Easing.Ease(RXEaseType, time, RXStart, RXChange, RXDuration) : 0,
            y = (RYActive) ? Easing.Ease(RYEaseType, time, RYStart, RYChange, RYDuration) : 0,
            z = (RZActive) ? Easing.Ease(RZEaseType, time, RZStart, RZChange, RZDuration) : 0
        };

        return euler;
    }

    /// <summary>
    /// Returns 1 for inactive values
    /// </summary>
    /// <param name="time"></param>
    /// <returns></returns>
    public Vector3 GetScale(float time)
    {
        Vector3 scale = new Vector3
        {
            x = (SXActive) ? Easing.Ease(SXEaseType, time, SXStart, SXChange, SXDuration) : 1,
            y = (SYActive) ? Easing.Ease(SYEaseType, time, SYStart, SYChange, SYDuration) : 1,
            z = (SZActive) ? Easing.Ease(SZEaseType, time, SZStart, SZChange, SZDuration) : 1
        };

        return scale;
    }
}
