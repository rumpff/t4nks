using UnityEngine;
using XboxCtrlrInput;

[CreateAssetMenu(fileName = "newPlayerProperties", menuName = "t4nk/Player Properties")]
public class PlayerProperties : ScriptableObject
{
    public string Name;
    public TankProperties Tank;
    public XboxController Controller;
}