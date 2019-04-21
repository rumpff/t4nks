using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCamera : MonoBehaviour
{

    [SerializeField] // The object that needs to be followed
    private Player m_Player; 
    private Camera m_Camera;

    [SerializeField] // The distance of the camera to the object
    private float m_BaseDistance;
    private float m_Distance;

    [SerializeField] // The angle of the camera on the Y axis
    [Range(0.0f, 89.99f)]
    private float m_BaseHeight;

    // Angleoffset for aiming
    private Vector2 m_AimOffset;

    [SerializeField] // The interpolation for all the lerps
    [Range(0.0f, 20.0f)]
    private float m_LerpInterpolation;

    // The desired angle
    private float m_angleDest;

    // The angle of the camera to the player
    private float m_ViewAngle;
    private float m_HeightAngle;

    private Vector3 m_BumpOffset;

    // The position of the camera
    private Vector3 m_Position;

    // The player's thingz
    private Vector3 m_PlayerPos;

    private void Start()
    {
        m_Camera = GetComponent<Camera>();
        m_Player.SetCamera(this);
    }

    private void FixedUpdate()
    {
        UpdateVariables();

        // Lerp the rotation
        m_ViewAngle = Mathf.LerpAngle(m_ViewAngle, m_angleDest + m_AimOffset.x, LerpInterpolation);
        m_HeightAngle = Mathf.LerpAngle(m_HeightAngle, m_BaseHeight + m_AimOffset.y, LerpInterpolation);
        m_Distance = m_BaseDistance;

        m_BumpOffset = Vector3.Lerp(m_BumpOffset, Vector3.zero, LerpInterpolation);

        // Calculate the final position
        m_Position.x = m_PlayerPos.x + Mathf.Sin((m_ViewAngle - 180) * Mathf.Deg2Rad) * (Mathf.Cos(m_HeightAngle * Mathf.Deg2Rad) * m_Distance);
        m_Position.y = m_PlayerPos.y + Mathf.Sin(m_HeightAngle * Mathf.Deg2Rad) * m_Distance;
        m_Position.z = m_PlayerPos.z + Mathf.Cos((m_ViewAngle - 180) * Mathf.Deg2Rad) * (Mathf.Cos(m_HeightAngle * Mathf.Deg2Rad) * m_Distance);

        m_Position += m_BumpOffset;

        // Apply the new values
        transform.position = m_Position;
        transform.LookAt(m_Player.transform);
    }

    private void UpdateVariables()
    {
        m_Position = transform.position;

        m_PlayerPos = m_Player.transform.position;

        // Only update the angle when the player is on the ground
        if (m_Player.IsOnGround())
            m_angleDest = m_Player.transform.eulerAngles.y;

        m_AimOffset = new Vector2(
            m_Player.Weapon.RawAim.x * 45.0f,
            m_Player.Weapon.RawAim.y * 20); // 15.0f
    }

    private float LerpInterpolation
    {
        get { return m_LerpInterpolation * Time.deltaTime; }
    }

    public Vector3 Bump
    {
        get { return m_BumpOffset; }
        set { m_BumpOffset = value; }
    }

    public Camera Camera
    {
        get { return m_Camera; }
    }
}