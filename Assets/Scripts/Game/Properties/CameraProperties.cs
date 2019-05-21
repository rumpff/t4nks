using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "newCameraProperties", menuName = "t4nk/Camera Properties")]
public class CameraProperties : ScriptableObject
{
    public float FOV;
    public float BaseDistance;
    public float BaseHeight;
    public float LerpInterpolation;
}
