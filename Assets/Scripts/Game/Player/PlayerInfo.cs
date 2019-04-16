using UnityEngine;
using XboxCtrlrInput;

[CreateAssetMenu(fileName = "newPlayerInfo", menuName = "t4nk/Player Info")]
public class PlayerInfo : ScriptableObject
{
    public string Name;
    public TankInfo Tank;
    public XboxController Controller;

    //ik ben lekker
}