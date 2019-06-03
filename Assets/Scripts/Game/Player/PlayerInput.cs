using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XboxCtrlrInput;

public class PlayerInput : MonoBehaviour
{
    private bool m_UsesKeyboard;
    private XboxController m_Controller;


    /// <summary>
    /// Initalize with controller input
    /// </summary>
    /// <param name="c"></param>
    public void Initalize(XboxController c)
    {
        if(c == XboxController.Any)
        {
            Initalize();
            return;
        }

        m_UsesKeyboard = false;
        m_Controller = c;

        InitalizePhaseTwo();
    }

    /// <summary>
    /// Initalize with keyboard input
    /// </summary>
    public void Initalize()
    {
        m_UsesKeyboard = true;
        m_Controller = XboxController.Any;

        InitalizePhaseTwo();
    }

    private void InitalizePhaseTwo()
    {

    }

    private IEnumerator ReadInputs()
    {
        while(true)
        {
            yield return new WaitForEndOfFrame();
        }
    }
}
