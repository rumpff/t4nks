using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Utill
{
    public static Color PlayerColor(int playerIndex)
    {
        float hue = ((playerIndex * 360.0f) / 4.0f) - 20;
        return Color.HSVToRGB(hue / 360.0f, 1, 1);
    }
}
