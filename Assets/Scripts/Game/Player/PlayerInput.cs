using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XboxCtrlrInput;

public enum InputType
{
    Controller,
    Keyboard
}

public class PlayerInput : MonoBehaviour
{
    private readonly float TriggerButtonThreshold = 0.35f;

    private InputType m_InputType;
    private XboxController m_Controller;


    /// <summary>
    /// Initalize with controller input
    /// </summary>
    /// <param name="controller"></param>
    public void Initalize(XboxController controller, InputType inputType)
    {
        m_InputType = inputType;
        m_Controller = controller;

        switch (inputType)
        {
            case InputType.Controller:
                StartCoroutine(UpdateWithController(controller));
                break;

            case InputType.Keyboard:
                StartCoroutine(UpdateWithKeyboard());
                break;
        }

    }

    private IEnumerator UpdateWithController(XboxController controller)
    {
        while(true)
        {
            DriveInput = XYStick(XboxAxis.LeftStickX, controller);
            TorqueOld =  Vector2.Distance(Vector2.zero, XYStick(XboxAxis.LeftStickX, controller)) * Mathf.Sign(XCI.GetAxis(XboxAxis.LeftStickY, controller) + 0.1f);
            Steer = XCI.GetAxis(XboxAxis.LeftStickX, controller);

            Brake = XCI.GetButton(XboxButton.LeftBumper, m_Controller);
            Jump = XCI.GetButtonDown(XboxButton.A, m_Controller);
            Shoot = (XCI.GetAxis(XboxAxis.RightTrigger, controller) >= TriggerButtonThreshold) ? true : false;
            Zoom = (XCI.GetAxis(XboxAxis.LeftTrigger, controller) >= TriggerButtonThreshold) ? true : false;

            Aim = InverseY(XYStick(XboxAxis.RightStickX, controller));
            AirControl = InverseY(XYStick(XboxAxis.LeftStickX, controller));

            yield return new WaitForEndOfFrame();
        }
    }

    private IEnumerator UpdateWithKeyboard()
    {
        Cursor.lockState = CursorLockMode.Locked;

        while (true)
        {
            Vector2 mouse = new Vector2 { x = Input.GetAxis("Mouse X"), y = -Input.GetAxis("Mouse Y") };
            Aim += mouse * 0.05f;
            Aim = Vector2.ClampMagnitude(Aim, 1.0f);

            Vector2 drivC = Vector2.zero;
            drivC.x -= Input.GetKey(KeyCode.A) ? 1 : 0;
            drivC.x += Input.GetKey(KeyCode.D) ? 1 : 0;
            drivC.y += Input.GetKey(KeyCode.W) ? 1 : 0;
            drivC.y -= Input.GetKey(KeyCode.S) ? 1 : 0;

            DriveInput = drivC;

            Vector2 airC = Vector2.zero;
            airC.x -= Input.GetKey(KeyCode.A) ? 1 : 0;
            airC.x += Input.GetKey(KeyCode.D) ? 1 : 0;
            airC.y -= Input.GetKey(KeyCode.W) ? 1 : 0;
            airC.y += Input.GetKey(KeyCode.S) ? 1 : 0;

            AirControl = airC;

            int TorqueDir = 0;
            if(Input.GetKey(KeyCode.W)) { TorqueDir++; }
            if (Input.GetKey(KeyCode.S)) { TorqueDir--; }

            int SteerDir = 0;
            if (Input.GetKey(KeyCode.D)) { SteerDir++; }
            if (Input.GetKey(KeyCode.A)) { SteerDir--; }

            TorqueOld = TorqueDir;// MoveTowardsLiniar(Torque, TorqueDir, -1, 1, 1.0f);
            Steer = SteerDir;// MoveTowardsLiniar(Steer, SteerDir, -1, 1, 1.0f);

            Brake = Input.GetKey(KeyCode.LeftShift);
            Jump = Input.GetKey(KeyCode.Space);
            Shoot = Input.GetMouseButton(0);
            Zoom = Input.GetMouseButton(1);

            yield return new WaitForEndOfFrame();
        }

        Cursor.lockState = CursorLockMode.None;
    }

    private Vector2 XYStick(XboxAxis xAsis, XboxController xboxController)
    {
        Vector2 output = new Vector2()
        {
            x = XCI.GetAxis(xAsis, xboxController),
            y = XCI.GetAxis(xAsis+1, xboxController),
        };

        return output;
    }

    private Vector2 InverseY(Vector2 value)
    {
        Vector2 output = value;
        output.y *= -1;
        return output;
    }

    private float MoveTowardsLiniar(float value, int direction, float min, float max, float speed)
    {
        float output = value;

        output += direction * Time.deltaTime * speed;
        output = Mathf.Clamp(value, min, max);

        return output;
    }

    public Vector2 DriveInput { get; private set; }
    public float TorqueOld { get; private set; }
    public float Steer { get; private set; }

    public Vector2 Aim { get; private set; }
    public Vector2 AirControl { get; private set; }

    public bool Brake { get; private set; }
    public bool Jump { get; private set; }
    public bool Shoot { get; private set; }
    public bool Zoom { get; private set; }
}
