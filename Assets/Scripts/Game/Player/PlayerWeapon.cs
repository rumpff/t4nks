using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XboxCtrlrInput;

public class PlayerWeapon : MonoBehaviour
{
    private readonly float m_ShootThreshold = 0.35f;
    private readonly Vector2 m_MaxAim = new Vector2(67.5f, 22.5f);

    private WeaponProperties m_EquippedWeapon;

    [SerializeField]
    private Transform m_TankHead, m_TankBarrel, m_BarrelEnd;

    private Player m_Player;

    private int m_CurrentAmmo;
    private float m_ShootCooldown;

    private Vector2 m_AimDirection;

    private void Awake()
    {
        m_Player = GetComponent<Player>();
    }


    private void Update()
    {
        ShootThing();
        TimerThing();
    }

    private void FixedUpdate()
    {
        AimThing();
    }

    private void AimThing()
    {
        m_AimDirection = Vector2.Lerp(m_AimDirection, new Vector2(
            m_Player.PInput.Aim.x * m_MaxAim.x, m_Player.PInput.Aim.y * m_MaxAim.y), 14.0f * Time.deltaTime);

        // Apply Rotation
        m_TankHead.localEulerAngles = new Vector3(0.0f, m_AimDirection.x, 0.0f);
        m_TankBarrel.localEulerAngles = new Vector3(m_AimDirection.y, 0.0f, 0.0f);
    }

    private void ShootThing()
    {
        if(m_Player.PInput.Shoot && m_ShootCooldown == 0)
        {
            Quaternion projectileRotation = Quaternion.Euler(m_TankBarrel.eulerAngles.x, m_TankHead.eulerAngles.y, m_Player.transform.eulerAngles.z);

            ProjectileBehaviour p = Instantiate(m_EquippedWeapon.Prefab, BarrelEnd.position, projectileRotation).GetComponent<ProjectileBehaviour>();

            p.Initalize(m_Player, m_EquippedWeapon, m_TankBarrel.forward);

            m_ShootCooldown = m_EquippedWeapon.Cooldown;
            m_Player.Camera.Bump = (-m_TankBarrel.forward * m_EquippedWeapon.CameraImpact);
            m_Player.AddForce(-m_TankBarrel.forward * m_EquippedWeapon.TankForce);

            // Check if the weapon does not have infinite ammo
            if (m_EquippedWeapon.StartingAmmo > 0)
            {
                m_CurrentAmmo--;

                if (m_CurrentAmmo <= 0)
                    ResetWeapon();
            }
        }
    }

    public void SwitchWeapon(WeaponProperties weapon)
    {
        m_EquippedWeapon = weapon;
        m_CurrentAmmo = weapon.StartingAmmo;
    }

    /// <summary>
    /// Assigns the default weapon
    /// </summary>
    public void ResetWeapon()
    {
        m_EquippedWeapon = m_Player.Properties.Tank.DefaultWeapon;
        m_CurrentAmmo = m_Player.Properties.Tank.DefaultWeapon.StartingAmmo;
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
        get { return new Vector2(m_AimDirection.x / m_MaxAim.x, m_AimDirection.y / m_MaxAim.y); }
    }

    public int Ammo
    {
        get { return m_CurrentAmmo; }
    }

    
    public Vector3 AimPosition()
    {
        Ray ray = new Ray(BarrelEnd.position, m_TankBarrel.forward);
        bool hasHit = Physics.Raycast(ray, out RaycastHit hit, 256.0f);

        if (hasHit)
            return hit.point;

        else // Defaults to x meters in front of tank if noting is hit
            return (m_TankBarrel.position + (m_TankBarrel.forward * 256.0f));
    }

    public Transform BarrelEnd
    {
        get { return m_BarrelEnd; }
    }

    public Vector3 BarrelBegin
    {
        get { return m_TankBarrel.position; }
    }

    public Vector3 TankHead
    {
        get { return m_TankHead.position; }
    }
}
