using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XboxCtrlrInput;

public class Player : MonoBehaviour
{
    private readonly string m_DriveableTag = "Driveable";
    private readonly string m_TireTag = "TankTire";

    [SerializeField]
    private Transform TankHead, TankBarrel;

    [SerializeField]
    private XboxController m_Controller;

    [SerializeField]
    private WheelCollider m_WheelFrontLeft, m_WheelFrontRight, m_WheelBackLeft, m_WheelBackRight;

    private Rigidbody m_RigidBody;

    [SerializeField]
    private TankInfo m_TankInfo;

    private Vector2 m_AimRotation;

    private float m_Torque = 0;
    private float m_BrakeTorque = 0;
    private float m_StreerAngle = 0;

    private float m_TorqueInput;
    private float m_StreerInput;
    private bool m_BrakeInput;
    private Vector2 m_AimInput;

    private void Start()
    {
        m_RigidBody = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        GetInput();
        CarThing();
        AimThing();
    }

    private void FixedUpdate()
    {
        ApplyCarMotion();
    }

    private void GetInput()
    {
        m_TorqueInput = XCI.GetAxis(XboxAxis.LeftStickY, m_Controller);
        m_StreerInput = XCI.GetAxis(XboxAxis.LeftStickX, m_Controller);
        m_BrakeInput = XCI.GetButton(XboxButton.X, m_Controller);
        m_AimInput = new Vector2(
            XCI.GetAxis(XboxAxis.RightStickX, m_Controller),
            -XCI.GetAxis(XboxAxis.RightStickY, m_Controller));
    }

    private void CarThing()
    {
        // Handle torque
        float torqueDest = m_TorqueInput * m_TankInfo.MaxTorque;

        if(m_Torque != torqueDest)
            m_Torque += Mathf.Sign(torqueDest - m_Torque) * (m_TankInfo.AccelerateRate * Time.deltaTime);

        // Handle steer
        m_StreerAngle = Mathf.Lerp(m_StreerAngle, m_StreerInput * m_TankInfo.MaxSteer, 12 * Time.deltaTime);

        // Handle brake
        m_BrakeTorque = (m_BrakeInput) ? m_TankInfo.BrakeTorque : 0;
    }

    private void AimThing()
    {
        m_AimRotation = Vector2.Lerp(m_AimRotation, new Vector2(
            m_AimInput.x * 90.0f, m_AimInput.y * 20.0f), 14.0f * Time.deltaTime);

        // Apply Rotation
        TankHead.localEulerAngles = new Vector3(0.0f, m_AimRotation.x, 0.0f);
        TankBarrel.localEulerAngles = new Vector3(m_AimRotation.y, 0.0f, 0.0f);
    }

    private void ApplyCarMotion()
    {
        // Driving
        m_WheelFrontLeft.motorTorque = m_Torque;
        m_WheelFrontRight.motorTorque = m_Torque;

        m_WheelBackLeft.motorTorque = m_Torque;
        m_WheelBackRight.motorTorque = m_Torque;

        // Steering
        m_WheelFrontLeft.steerAngle = m_StreerAngle;
        m_WheelFrontRight.steerAngle = m_StreerAngle;

        m_WheelBackLeft.steerAngle = -m_StreerAngle;
        m_WheelBackRight.steerAngle = -m_StreerAngle;

        // Handbrake
        m_WheelFrontLeft.brakeTorque = m_BrakeTorque;
        m_WheelFrontRight.brakeTorque = m_BrakeTorque;

        m_WheelBackLeft.brakeTorque = m_BrakeTorque;
        m_WheelBackRight.brakeTorque = m_BrakeTorque;
    }
}