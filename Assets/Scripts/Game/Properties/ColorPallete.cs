using UnityEngine;

[CreateAssetMenu(fileName = "newColorPallete", menuName = "t4nk/Color Pallete")]
public class ColorPallete : ScriptableObject
{
    public enum PalleteColor
    {
        Primary,
        Secundary,
        Accesory,
        Highlight
    };

    public string Name;
    [Space(10)]
    public Color Primary;
    public Color Secundary;
    [Space(4)]
    public Color Accesories;
    public Color Highlight;
}
