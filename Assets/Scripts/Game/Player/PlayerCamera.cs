using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCamera : MonoBehaviour
{

    [SerializeField] // The object that needs to be followed
    private Player m_Player;                                        

    [SerializeField] // The distance of the camera to the object
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

    // The positions of the camera
    private float m_X, m_Y, m_Z;

    // The player's thingz
    private Vector3 m_PlayerPos;

    void FixedUpdate()
    {
        UpdateVariables();

        // Lerp therotation
        m_ViewAngle = Mathf.LerpAngle(m_ViewAngle, m_angleDest + m_AimOffset.x, LerpInterpolation);
        m_HeightAngle = Mathf.LerpAngle(m_HeightAngle, m_BaseHeight + m_AimOffset.y, LerpInterpolation);

        // Calculate the final position
        m_X = m_PlayerPos.x + Mathf.Sin((m_ViewAngle - 180) * Mathf.Deg2Rad) * (Mathf.Cos(m_HeightAngle * Mathf.Deg2Rad) * m_Distance);
        m_Y = m_PlayerPos.y + Mathf.Sin(m_HeightAngle * Mathf.Deg2Rad) * m_Distance;
        m_Z = m_PlayerPos.z + Mathf.Cos((m_ViewAngle - 180) * Mathf.Deg2Rad) * (Mathf.Cos(m_HeightAngle * Mathf.Deg2Rad) * m_Distance);

        // Apply the new values
        transform.position = new Vector3(m_X, m_Y, m_Z);
        transform.LookAt(m_Player.transform);
    }

    private void UpdateVariables()
    {
        m_X = transform.position.x;
        m_Y = transform.position.y;
        m_Z = transform.position.z;

        m_PlayerPos = m_Player.transform.position;

        // Only update the angle when the player is on the ground
        if (m_Player.IsOnGround())
            m_angleDest = m_Player.transform.eulerAngles.y;

        m_AimOffset = new Vector2(
            m_Player.PlayerWeapon.RawAim.x * 45.0f,
            m_Player.PlayerWeapon.RawAim.y * 20); // 15.0f
    }

    private float LerpInterpolation
    {
        get { return m_LerpInterpolation * Time.deltaTime; }
    }
}