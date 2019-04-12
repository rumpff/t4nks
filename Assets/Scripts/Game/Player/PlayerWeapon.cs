using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XboxCtrlrInput;

public class PlayerWeapon : MonoBehaviour
{
    private readonly float m_ShootThreshold = 0.35f;
    private readonly Vector2 m_MaxAim = new Vector2(67.5f, 22.5f);

    [SerializeField]
    private Transform m_TankHead, m_TankBarrel;
    [SerializeField]
    private GameObject m_ProjectilePrefab;

    private Player m_Player;

    private bool m_ShootInput;
    private Vector2 m_AimInput;

    private float m_ShootCooldown;

    private Vector2 m_AimRotation;

    private void Awake()
    {
        m_Player = GetComponent<Player>();
    }


    private void Update()
    {
        GetInput();
        AimThing();

        TimerThing();
    }

    private void GetInput()
    {
        m_ShootInput = (XCI.GetAxis(XboxAxis.RightTrigger, m_Player.Controller) >= m_ShootThreshold) ? true : false;
        m_AimInput = new Vector2(
            XCI.GetAxis(XboxAxis.RightStickX, m_Player.Controller),
            -XCI.GetAxis(XboxAxis.RightStickY, m_Player.Controller));
    }

    private void AimThing()
    {
        m_AimRotation = Vector2.Lerp(m_AimRotation, new Vector2(
            m_AimInput.x * m_MaxAim.x, m_AimInput.y * m_MaxAim.y), 14.0f * Time.deltaTime);

        // Apply Rotation
        m_TankHead.localEulerAngles = new Vector3(0.0f, m_AimRotation.x, 0.0f);
        m_TankBarrel.localEulerAngles = new Vector3(m_AimRotation.y, 0.0f, 0.0f);
    }

    private void ShootThing()
    {
        if(m_ShootInput && m_ShootCooldown == 0)
        {
            Projectile p = Instantiate(m_ProjectilePrefab)
        }
    }

    private void TimerThing()
    {
        if (m_ShootCooldown > 0)
            m_ShootCooldown -= Time.deltaTime;
        if (m_ShootCooldown < 0)
            m_ShootCooldown = 0;
    }

    /// <summary>
    /// Absolute aim rotation
    /// </summary>
    public Vector2 RawAim
    {
        get { return new Vector2(m_AimRotation.x / m_MaxAim.x, m_AimRotation.y / m_MaxAim.y); }
    }
}
