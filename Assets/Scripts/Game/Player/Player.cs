using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XboxCtrlrInput;

public class Player : MonoBehaviour
{
    private readonly string m_DriveableTag = "Driveable";
    private readonly string m_TireTag = "TankTire";
    private readonly Vector2 m_MaxAim = new Vector2(67.5f, 22.5f);

    [SerializeField]
    private Transform m_TankHead, m_TankBarrel;

    private PlayerWeapon m_PlayerWeapon;

    private PlayerCamera m_Camera;

    [SerializeField]
    private XboxController m_Controller;

    [SerializeField]
    private WheelCollider m_WheelFrontLeft, m_WheelFrontRight, m_WheelBackLeft, m_WheelBackRight;
    private List<WheelHit> m_WheelHits;

    private Rigidbody m_RigidBody;

    [SerializeField]
    private TankInfo m_TankInfo;

    [SerializeField]
    private GameObject[] m_DeathObjects;

    private Vector2 m_AimRotation;

    private float m_Torque = 0;
    private float m_BrakeTorque = 0;
    private float m_StreerAngle = 0;

    private float m_TorqueInput;
    private float m_StreerInput;
    private bool m_BrakeInput;
    private bool m_JumpInput;
    private Vector2 m_AimInput;
    private Vector2 m_AirControlInput;

    private int m_JumpsLeft = 0;

    private void Awake()
    {
        m_WheelHits = new List<WheelHit>();
        m_PlayerWeapon = GetComponent<PlayerWeapon>();
        m_RigidBody = GetComponent<Rigidbody>();
    }

    private void Start()
    {
        StartCoroutine(ReadWheelHits());
    }

    private void Update()
    {
        ReadWheelHits();
        GetInput();

        CarThing();
        //AimThing();

        TimerThing();
    }

    private void FixedUpdate()
    {
        ApplyCarMotion();
        AircontrolThing();
        JumpThing();
    }

    private void OnDestroy()
    {
        foreach (GameObject g in m_DeathObjects)
        {
            Instantiate(g, transform.position, Quaternion.Euler(0, 0, 0));
        }

        //m_Camera.
    }

    private void GetInput()
    {
        m_TorqueInput = XCI.GetAxis(XboxAxis.LeftStickY, m_Controller);
        m_StreerInput = XCI.GetAxis(XboxAxis.LeftStickX, m_Controller);

        m_BrakeInput = XCI.GetButton(XboxButton.LeftBumper, m_Controller);
        m_JumpInput = XCI.GetButtonDown(XboxButton.A, m_Controller);

        m_AimInput = new Vector2(
            XCI.GetAxis(XboxAxis.RightStickX, m_Controller),
            -XCI.GetAxis(XboxAxis.RightStickY, m_Controller));

        m_AirControlInput = new Vector2(
            XCI.GetAxis(XboxAxis.LeftStickX, m_Controller),
            -XCI.GetAxis(XboxAxis.LeftStickY, m_Controller));
    }

    private void CarThing()
    {
        // Handle torque
        float torqueDest = 0;

        if(IsOnGround())
            torqueDest = m_TorqueInput * m_TankInfo.MaxTorque;

        if (m_Torque != torqueDest)
        {
            float changeDir = Mathf.Sign(torqueDest - m_Torque);
            m_Torque += changeDir * (m_TankInfo.AccelerateRate * Time.deltaTime);

            // Prevent overshooting
            if(changeDir == 1 && (m_Torque > torqueDest) ||
                changeDir == -1 && (m_Torque < torqueDest))
            {
                m_Torque = torqueDest;
            }
        }



        // Handle steer
        m_StreerAngle = Mathf.Lerp(m_StreerAngle, m_StreerInput * m_TankInfo.MaxSteer, 12 * Time.deltaTime);

        // Handle brake
        m_BrakeTorque = (m_BrakeInput) ? m_TankInfo.BrakeTorque : 0;
    }

    private void AimThing()
    {
        m_AimRotation = Vector2.Lerp(m_AimRotation, new Vector2(
            m_AimInput.x * m_MaxAim.x, m_AimInput.y * m_MaxAim.y), 14.0f * Time.deltaTime);

        // Apply Rotation
        m_TankHead.localEulerAngles = new Vector3(0.0f, m_AimRotation.x, 0.0f);
        m_TankBarrel.localEulerAngles = new Vector3(m_AimRotation.y, 0.0f, 0.0f);
    }

    private void AircontrolThing()
    {
        if (IsOnGround())
            return; // Not in the air

        m_RigidBody.AddTorque(transform.up * (m_AirControlInput.x * m_TankInfo.AirControl));
        m_RigidBody.AddTorque(-transform.right * (m_AirControlInput.y * m_TankInfo.AirControl));
    }

    private void JumpThing()
    {

        if (m_JumpInput)
        {
            if (m_JumpsLeft <= 0)
                return;

            Vector3 jumpDirection = GetGroundNormal(transform.up);

            m_RigidBody.AddForce(jumpDirection * m_TankInfo.JumpForce);

            m_JumpsLeft--;
        }
    }

    private void TimerThing()
    {
        // Jumping
        if (IsOnGround())
            m_JumpsLeft = m_TankInfo.AirJumpAmount;
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

    private IEnumerator ReadWheelHits()
    {
        while(true)
        {
            yield return new WaitForFixedUpdate();

            m_WheelHits = new List<WheelHit>();
            WheelHit hit;

            if (m_WheelFrontLeft.GetGroundHit(out hit))
                m_WheelHits.Add(hit);

            if (m_WheelFrontRight.GetGroundHit(out hit))
                m_WheelHits.Add(hit);

            if (m_WheelBackLeft.GetGroundHit(out hit))
                m_WheelHits.Add(hit);

            if (m_WheelBackRight.GetGroundHit(out hit))
                m_WheelHits.Add(hit);
        }

    }

    /// <summary>
    /// If any of the tank's wheels are on the ground
    /// </summary>
    /// <returns></returns>
    public bool IsOnGround()
    {
        return (m_WheelHits.Count != 0);
    }

    /// <summary>
    /// Returns normal of the ground, or the default when in the air
    /// </summary>
    /// <returns></returns>
    public Vector3 GetGroundNormal(Vector3 defaultValue)
    {
        Vector3 normal = Vector3.zero;

        if (IsOnGround())
        {
            // Get the average direction from all the surfaces the wheels are touching
            // eg when with only 2 wheels on a slope
            for (int i = 0; i < m_WheelHits.Count; i++)
            {
                normal += m_WheelHits[i].normal;
            }

            normal /= m_WheelHits.Count;
        }
        else
        {
            // We are in the air so we take the direction of the tank
            normal = defaultValue;
        }

        return normal;
    }

    public void SetCamera(PlayerCamera c)
    {
        m_Camera = c;
    }

    public void AddForce(Vector3 force)
    {
        m_RigidBody.AddForce(force);
    }

    /// <summary>
    /// Absolute aim rotation
    /// </summary>
    public Vector2 RawAim
    {
        get { return new Vector2(m_AimRotation.x / m_MaxAim.x, m_AimRotation.y / m_MaxAim.y); }
    }

    public XboxController Controller
    {
        get { return m_Controller; }
    }

    public PlayerWeapon PlayerWeapon
    {
        get { return m_PlayerWeapon; }
    }

    public PlayerCamera Camera
    {
        get { return m_Camera; }
    }
}