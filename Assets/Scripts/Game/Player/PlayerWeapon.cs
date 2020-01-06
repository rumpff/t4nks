using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimpleEasing;
using XboxCtrlrInput;

public class PlayerWeapon : MonoBehaviour
{
    private readonly float m_ShootThreshold = 0.35f;
    private readonly Vector2 m_MaxAim = new Vector2(67.5f, 22.5f);

    private WeaponProperties m_EquippedWeapon;

    [SerializeField]
    private Transform m_TankHead, m_BarrelEnd, m_BarrelParent, m_Barrel;

    private Player m_Player;

    private int m_CurrentAmmo;
    private float m_ShootCooldown;
    private float m_BarrelAnimationTimer;

    private Vector2 m_AimDirection;
    private Vector2 m_SpreadDirection;

    private void Awake()
    {
        m_Player = GetComponent<Player>();
        m_SpreadDirection = new Vector2();
    }


    private void Update()
    {
        ShootThing();
        TimerThing();
        BarrelAnimation();
    }

    private void FixedUpdate()
    {
        AimThing();
    }

    private void AimThing()
    {
        m_AimDirection = Vector2.Lerp(m_AimDirection, new Vector2(
            m_Player.PInput.Aim.x * m_MaxAim.x, m_Player.PInput.Aim.y * m_MaxAim.y), 14.0f * Time.deltaTime) + m_SpreadDirection;

        // Apply Rotation
        m_TankHead.localEulerAngles = new Vector3(0.0f, m_AimDirection.x, 0.0f);
        TankBarrel.localEulerAngles = new Vector3(m_AimDirection.y, 0.0f, 0.0f);
    }

    private void ShootThing()
    {
        if(m_Player.PInput.Shoot && m_ShootCooldown == 0)
        {
            Vector3 angle = new Vector3(TankBarrel.eulerAngles.x, m_TankHead.eulerAngles.y, m_Player.transform.eulerAngles.z);
            Quaternion projectileRotation = Quaternion.Euler(angle);

            ProjectileBehaviour p = Instantiate(m_EquippedWeapon.ProjectilePrefab, BarrelEnd.position, projectileRotation).GetComponent<ProjectileBehaviour>();

            //p.Initalize(m_Player, m_EquippedWeapon, TankBarrel.forward);

            p.Initalize(m_Player, m_EquippedWeapon, TankBarrel.forward);

            m_ShootCooldown = m_EquippedWeapon.Cooldown;
            m_Player.Camera.Bump = (-TankBarrel.forward * m_EquippedWeapon.CameraImpact);
            m_Player.AddForce(-TankBarrel.forward * m_EquippedWeapon.TankForce);

            AddSpread(m_EquippedWeapon.SpreadAngle);

            // Reset the timer which makes the animation play
            m_BarrelAnimationTimer = 0;

            // Check if the weapon does not have infinite ammo
            if (m_EquippedWeapon.StartingAmmo > 0)
            {
                m_CurrentAmmo--;

                if (m_CurrentAmmo <= 0)
                    ResetWeapon();
            }
        }
    }

    private void BarrelAnimation()
    {
        float time = m_BarrelAnimationTimer;
        TransformAnimation anim = m_EquippedWeapon.ReloadAnimation;

        // Apply values
        m_Barrel.localPosition = anim.GetTranslate(time);
        m_Barrel.localEulerAngles = anim.GetEuler(time);
        m_Barrel.localScale = anim.GetScale(time);


        // Timer
        m_BarrelAnimationTimer += Time.deltaTime;
    }

    /// <summary>
    /// Override the current weapon with a new weapon
    /// </summary>
    /// <param name="newWeapon"></param>
    public void SwitchWeapon(WeaponProperties newWeapon)
    {
        // Swap out weapons
        m_EquippedWeapon = newWeapon;

        // Remove the old barrel objects
        foreach(Transform obj in m_BarrelParent.GetComponentInChildren<Transform>())
        {
            if(obj != null)
                Destroy(obj.gameObject);
        }

        // Spawn new barrel mesh
        GameObject newBarrel = Instantiate(newWeapon.BarrelPrefab, TankBarrel);

        // Assign the new gameobjects
        m_BarrelEnd = newBarrel.transform.Find("BarrelEnd");
        m_Barrel = newBarrel.transform;

        // Update layermasks
        m_Player.UpdatePlayerLayerMasks(newBarrel.GetComponentsInChildren<MeshRenderer>(), m_Player.Index);
        
        // Update ammo and etc. values
        m_CurrentAmmo = newWeapon.StartingAmmo;
    }
    /// <summary>
    /// Assigns the default weapon
    /// </summary>
    public void ResetWeapon()
    {
        SwitchWeapon(m_Player.Properties.Tank.DefaultWeapon);
    }

    private void TimerThing()
    {
        if (m_ShootCooldown > 0)
            m_ShootCooldown -= Time.deltaTime;
        if (m_ShootCooldown < 0)
            m_ShootCooldown = 0;

        m_SpreadDirection = Vector2.Lerp(m_SpreadDirection, Vector2.zero, 7 * Time.deltaTime);
    }

    /// <summary>
    /// Absolute aim rotation
    /// </summary>
    public Vector2 RawAim
    {
        get { return m_AimDirection / m_MaxAim; }
    }

    /// <summary>
    /// Absolute aim rotation without spread
    /// </summary>
    public Vector2 AbsuluteAim
    {
        get { return (m_AimDirection - m_SpreadDirection) / m_MaxAim.x; }
    }

    public int Ammo
    {
        get { return m_CurrentAmmo; }
    }

    
    public Vector3 AimPosition()
    {
        Ray ray = new Ray(BarrelEnd.position, TankBarrel.forward);
        bool hasHit = Physics.Raycast(ray, out RaycastHit hit, 4040.0f);

        if (hasHit)
            return hit.point;

        else // Defaults to x meters in front of tank if noting is hit
            return (TankBarrel.position + (TankBarrel.forward * 4040.0f));
    }

    public float AimDistance()
    {
        float distance = Vector3.Distance(BarrelEnd.position, AimPosition());

        if (distance > 4000)
            distance = 0;

        return distance;
    }

    public void AddSpread(float angle)
    {
        float yScale = m_MaxAim.y / (m_MaxAim.x / 2.0f);

        m_SpreadDirection.x += Random.Range(-angle, angle);
        m_SpreadDirection.y += Random.Range(-angle * yScale, -0);
    }

    public Transform TankBarrel
    {
        get { return m_BarrelParent; }
    }

    public Transform BarrelEnd
    {
        get { return m_BarrelEnd; }
    }

    public Vector3 BarrelBegin
    {
        get { return TankBarrel.position; }
    }

    public Vector3 TankHead
    {
        get { return m_TankHead.position; }
    }

    public Vector2 Spread
    {
        get { return m_SpreadDirection; }
    }
}
