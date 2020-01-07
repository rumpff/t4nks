using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XboxCtrlrInput;
using TMPro;
using static UnityEngine.ParticleSystem;

public class Player : MonoBehaviour
{
    private const string DriveableTag = "Driveable";
    private const string TireTag = "TankTire";
    private const float JumpCooldown = 0.2f;
    public readonly Vector2 MaxAim = new Vector2(67.5f, 22.5f);

    private GameManager m_GameManager;

    public delegate void Destroyed(int playerId, Player Destroyer);
    public event Destroyed DestroyedEvent;

    private int m_PlayerIndex;
    private PlayerProperties m_PlayerProperties;
    private TankProperties m_TankProperties;

    public PlayerHealth Health { get; private set; }
    public PlayerWeapon Weapon { get; private set; }
    public PlayerCamera Camera { get; private set; }

    [SerializeField]
    private Transform m_TankHead, m_TankBarrel;

    [SerializeField]
    private XboxController m_Controller;

    [SerializeField]
    private WheelCollider m_WheelLeftFront, m_WheelRightFront, m_WheelLeftBack, m_WheelRightBack;
    private List<WheelCollider> m_WheelsLeft, m_WheelsRight;
    private List<WheelHit> m_WheelHits;

    [SerializeField]
    private Transform m_GroundSensor;
    [SerializeField]
    private LayerMask m_GroundSensorIgnore;

    private Vector2 m_AimRotation;

    private Vector2 m_Torque = Vector2.zero;
    private float m_TorqueOld = 0;
    private float m_BrakeTorque = 0;
    private float m_StreerAngle = 0;

    private float m_JumpTimer = 0;
    private int m_JumpsLeft = 0;

    private float m_PlayerHeightOffset;

    private Transform m_TireLeft, m_TireRight;

    private Transform m_JumpThruster;
    private Vector3 m_JumpThrusterDefaultPos;
    private float m_JumpThrusterAnimationTimer;

    private float m_previousVelocity;
    private float m_currentVelocity;

    private enum ParticleType
    {
        JumpThrusterEmission,
        JumpThrusterExplosion
    }

    private Dictionary<ParticleType, ParticleSystem> m_ParticleSystems;
    private Dictionary<ParticleType, float> m_ParticleEmission;

    private void Awake()
    {
        Weapon = GetComponent<PlayerWeapon>();
        Health = GetComponent<PlayerHealth>();
        Rigidbody = GetComponent<Rigidbody>();
        PInput = GetComponent<PlayerInput>();

        m_WheelHits = new List<WheelHit>();
    }

    public void Initalize(int playerIndex, PlayerProperties playerProperties, PlayerCamera playerCamera, GameManager gameManager)
    {
        m_PlayerIndex = playerIndex;
        m_PlayerProperties = playerProperties;
        Camera = playerCamera;
        m_GameManager = gameManager;

        // Assign properties
        m_Controller = m_PlayerProperties.Controller;
        m_TankProperties = m_PlayerProperties.Tank;

        // Initalize input
        PInput.Initalize(m_PlayerProperties.Controller, playerProperties.InputType);

        // Initalize health
        Health.InitalizeHealth(m_TankProperties.MaxHealth);
        Health.DeathEvent += OnDeath;
        Health.DamageEvent += OnDamage;

        // Initalize weapon
        Weapon.SwitchWeapon(playerProperties.Tank.DefaultWeapon);

        StartCoroutine(ReadWheelHits());

        // Wheels
        WheelCollider[] wheels = transform.GetComponentsInChildren<WheelCollider>();
        m_WheelsLeft = new List<WheelCollider>();
        m_WheelsRight = new List<WheelCollider>();

        foreach (WheelCollider wheel in wheels)
        {
            string parent = wheel.transform.parent.name.ToLower();
            if(parent.Contains("left"))
            {
                m_WheelsLeft.Add(wheel);
            }

            if (parent.Contains("right"))
            {
                m_WheelsRight.Add(wheel);
            }
        }

        m_TireLeft = transform.Find("Mesh/TireLeft");
        m_TireRight = transform.Find("Mesh/TireRight");

        // Set the layer masks
        MeshRenderer[] meshObjects = transform.GetComponentsInChildren<MeshRenderer>();
        UpdatePlayerLayerMasks(meshObjects, playerIndex);

        // Find and store all particle systems
        ParticleSystem[] particleSystems = transform.GetComponentsInChildren<ParticleSystem>();
        m_ParticleSystems = new Dictionary<ParticleType, ParticleSystem>();
        m_ParticleEmission = new Dictionary<ParticleType, float>();

        foreach (ParticleSystem p in particleSystems)
        {
            foreach(ParticleType type in (ParticleType[])Enum.GetValues(typeof(ParticleType)))
            {
                if (p.transform.name.Contains(type.ToString()))
                {
                    m_ParticleSystems.Add(type, p);
                    m_ParticleEmission.Add(type, 0.0f);

                    break;
                }
            }
        }

        // JumpThruster

        m_JumpThruster = transform.Find("Mesh/JumpThruster");
        m_JumpThrusterDefaultPos = m_JumpThruster.localPosition;
        m_JumpThrusterAnimationTimer = 9999.0f;
    }

    private void Update()
    {
        CarUpdate();
        VisualsAndEffectsUpdate();
        TimerUpdate();

        if (m_PlayerIndex == 0 && Input.GetKey(KeyCode.T))
            Health.DamagePlayer(30, this);
    }

    private void FixedUpdate()
    {
        ApplyCarMotion();
        AircontrolUpdate();
        JumpUpdate();
    }

    public void UpdatePlayerLayerMasks(MeshRenderer[] meshObjects, int playerIndex)
    {
        for (int i = 0; i < meshObjects.Length; i++)
        {
            if (meshObjects[i].transform.name.Contains("[TPOnly]"))
            {
                // Set the Third person only layer masks
                meshObjects[i].gameObject.layer = 11 + playerIndex;
            }
        }
    }

    /*  Tank  */
    private void CarUpdate()
    {
        // Handle torque
        #region old torque
        float torqueDest = 0;

        if(IsOnGround())
            torqueDest = PInput.TorqueOld * m_TankProperties.MaxTorque;

        if (m_TorqueOld != torqueDest)
        {
            float changeDir = Mathf.Sign(torqueDest - m_TorqueOld);
            m_TorqueOld += changeDir * (m_TankProperties.AccelerateRate * Time.deltaTime);

            // Prevent overshooting
            if(changeDir == 1 && (m_TorqueOld > torqueDest) ||
                changeDir == -1 && (m_TorqueOld < torqueDest))
            {
                m_TorqueOld = torqueDest;
            }
        }
        #endregion

        #region new torque
        /*
        float controlDirection = Mathf.Atan2(PInput.DriveInput.x, PInput.DriveInput.y) * Mathf.Rad2Deg;
        float controlMagnitude = Vector2.Distance(Vector2.zero, PInput.DriveInput);

        KAK.text = controlDirection.ToString();
        */
        #endregion

        // Handle steer
        m_StreerAngle = Mathf.Lerp(m_StreerAngle, PInput.Steer * m_TankProperties.MaxSteer, 12 * Time.deltaTime);

        // Handle brake
        m_BrakeTorque = (PInput.Brake) ? m_TankProperties.BrakeTorque : 0;
    }
    private void AircontrolUpdate()
    {
        if (IsOnGround())
            return; // Not in the air

        Rigidbody.AddTorque(transform.up * (PInput.AirControl.x * m_TankProperties.AirControl));
        Rigidbody.AddTorque(-transform.right * (PInput.AirControl.y * m_TankProperties.AirControl));
    }
    private void JumpUpdate()
    {
        if (PInput.Jump && m_JumpTimer <= 0)
        {
            if (m_JumpsLeft <= 0)
                return;

            Vector3 jumpDirection = GetGroundNormal(transform.up);

            Rigidbody.AddForce(jumpDirection * m_TankProperties.JumpForce);

            m_JumpsLeft--;
            m_JumpTimer = JumpCooldown;

            if(IsOnGround())
            {
                m_ParticleEmission[ParticleType.JumpThrusterExplosion] = 10000;
            }

            m_JumpThrusterAnimationTimer = 0;
            m_ParticleEmission[ParticleType.JumpThrusterEmission] = 500;
        }
    }
    private void TimerUpdate()
    {
        // Jumping
        if (IsOnGround())
            m_JumpsLeft = m_TankProperties.AirJumpAmount;
        m_JumpTimer -= Time.deltaTime;

        // Visuals and Effects
        m_JumpThrusterAnimationTimer += Time.deltaTime;
    }
    private void ParticleUpdate()
    {
        // Apply Emissionrates
        foreach (ParticleType type in (ParticleType[])Enum.GetValues(typeof(ParticleType)))
        {
            EmissionModule JumpThrust = m_ParticleSystems[type].emission;
            JumpThrust.rateOverTime = m_ParticleEmission[type];
        }

        // Bring emission rates to 0
        m_ParticleEmission[ParticleType.JumpThrusterEmission] = Mathf.Lerp(m_ParticleEmission[ParticleType.JumpThrusterEmission], 0, 8 * Time.deltaTime);
        m_ParticleEmission[ParticleType.JumpThrusterExplosion] = Mathf.Lerp(m_ParticleEmission[ParticleType.JumpThrusterExplosion], 0, 100 * Time.deltaTime);
    }
    private void ApplyCarMotion()
    {
        // Driving
        m_WheelLeftFront.motorTorque = m_TorqueOld;
        m_WheelRightFront.motorTorque = m_TorqueOld;

        m_WheelLeftBack.motorTorque = m_TorqueOld;
        m_WheelRightBack.motorTorque = m_TorqueOld;

        // Steering
        m_WheelLeftFront.steerAngle = m_StreerAngle;
        m_WheelRightFront.steerAngle = m_StreerAngle;

        m_WheelLeftBack.steerAngle = -m_StreerAngle;
        m_WheelRightBack.steerAngle = -m_StreerAngle;

        // Handbrake
        m_WheelLeftFront.brakeTorque = m_BrakeTorque;
        m_WheelRightFront.brakeTorque = m_BrakeTorque;

        m_WheelLeftBack.brakeTorque = m_BrakeTorque;
        m_WheelRightBack.brakeTorque = m_BrakeTorque;
    }
    private void VisualsAndEffectsUpdate()
    {
        // Tire positions
        SetTirePos(ref m_TireLeft, m_WheelLeftFront, m_WheelLeftBack);
        SetTirePos(ref m_TireRight, m_WheelRightFront, m_WheelRightBack);

        ParticleUpdate();

        // JumpThruster
        TransformAnimation JTanim = m_TankProperties.JumpThrusterAnimation;

        m_JumpThruster.localPosition = JTanim.GetTranslate(m_JumpThrusterAnimationTimer) + m_JumpThrusterDefaultPos;
        m_JumpThruster.localEulerAngles = JTanim.GetEuler(m_JumpThrusterAnimationTimer);
        m_JumpThruster.localScale = JTanim.GetScale(m_JumpThrusterAnimationTimer);

        // Camera shake on impact
        m_previousVelocity = m_currentVelocity;
        m_currentVelocity = Vector3.Distance(Vector3.zero, Rigidbody.velocity);

        float difference = m_previousVelocity - m_currentVelocity;

        if (difference > 10)
            Camera.AddScreenshake(0.02f * difference);
    }
    private void SetTirePos(ref Transform tire, WheelCollider frontWheel, WheelCollider backWheel)
    {
        Vector3 frontPos = WheelPosition(frontWheel);
        Vector3 backPos = WheelPosition(backWheel);

        Vector3 pos = Vector3.Lerp(frontPos, backPos, 0.5f);
        Quaternion rotation =  Quaternion.LookRotation(frontPos - pos, transform.up);

        pos.y += m_TankProperties.TireGroundOffset;

        // Apply values
        tire.position = pos;
        tire.rotation = rotation;
    }
    private void ExplodeTank()
    {
        MeshFilter[] meshes = GetComponentsInChildren<MeshFilter>();
        List<GameObject> debris = new List<GameObject>();

        // Copy the tank in loose parts
        for (int i = 0; i < meshes.Length; i++)
        {
            GameObject o = new GameObject(("Player" + m_PlayerIndex + "debris part"));

            o.transform.position = meshes[i].transform.position;
            o.transform.rotation = meshes[i].transform.rotation;
            o.transform.localScale = meshes[i].transform.localScale;

            o.AddComponent<MeshFilter>().mesh = meshes[i].mesh;
            o.AddComponent<MeshRenderer>().materials = meshes[i].GetComponent<MeshRenderer>().materials;
            o.AddComponent<BoxCollider>();

            Rigidbody rb = o.AddComponent<Rigidbody>();
            o.AddComponent<Explodable>();

            rb.mass = 1200;
            rb.velocity = Rigidbody.velocity;
            rb.angularVelocity = Rigidbody.angularVelocity;

            // Randomize velocity abit
            rb.velocity += V3RandomRange(-7, 7);
            rb.angularVelocity += V3RandomRange(-500, 500);

            debris.Add(o);
        }

        // Create the explosion
        Explosion e = Instantiate(m_TankProperties.ExplosionPrefab).GetComponent<Explosion>();
        e.transform.position = transform.position;
        e.Initalize(this, m_TankProperties.DestroyExplosionProperties, new HitProperties(false));
    }

    private void OnDeath(Player damager)
    {
        if (DestroyedEvent != null)
            DestroyedEvent.Invoke(m_PlayerIndex, damager);

        DestroyedEvent = null;

        ExplodeTank();
        Destroy(gameObject);
    }

    public void OnDamage(float damageAmount)
    {
        m_GameManager.Players[m_PlayerIndex].UI.AddDamageEffect(damageAmount, m_PlayerProperties.Tank.MaxHealth);
    }

    private IEnumerator ReadWheelHits()
    {
        while(true)
        {
            yield return new WaitForFixedUpdate();

            m_WheelHits = new List<WheelHit>();
            WheelHit hit;

            if (m_WheelLeftFront.GetGroundHit(out hit))
                m_WheelHits.Add(hit);

            if (m_WheelRightFront.GetGroundHit(out hit))
                m_WheelHits.Add(hit);

            if (m_WheelLeftBack.GetGroundHit(out hit))
                m_WheelHits.Add(hit);

            if (m_WheelRightBack.GetGroundHit(out hit))
                m_WheelHits.Add(hit);
        }

    }

    private Vector3 WheelPosition(WheelCollider wheel)
    {
        RaycastHit hit;
        Vector3 wheelCenter = wheel.transform.TransformPoint(wheel.center);

        if (Physics.Raycast(wheelCenter, -wheel.transform.up, out hit, wheel.suspensionDistance + wheel.radius))
        {
            return hit.point + (wheel.transform.up * wheel.radius);
        }
        else
        {
            return wheelCenter - (wheel.transform.up * wheel.suspensionDistance);
        }
    }

    /// <summary>
    /// If any of the tank's wheels are on the ground
    /// </summary>
    /// <returns></returns>
    public bool IsOnGround()
    {
        return (m_WheelHits.Count > 2);
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

    public float GetHeightFromGround()
    {
        float height = 0;
        RaycastHit hit;

        Physics.Raycast(m_GroundSensor.position, Vector3.down, out hit, float.PositiveInfinity, m_GroundSensorIgnore);
        height = hit.distance;

        return height;
    }

    public Vector3 V3RandomRange(float min, float max)
    {
        System.Random rand = new System.Random(Guid.NewGuid().GetHashCode());

        Vector3 output = new Vector3
        {
            x = (float)rand.Next((int)min * 10000, (int)max * 10000) / 10000.0f,
            y = (float)rand.Next((int)min * 10000, (int)max * 10000) / 10000.0f,
            z = (float)rand.Next((int)min * 10000, (int)max * 10000) / 10000.0f,
        };

        return output;
    }

    public void SetCamera(PlayerCamera c)
    {
        Camera = c;
    }

    public void AddForce(Vector3 force)
    {
        Rigidbody.AddForce(force);
    }

    public XboxController Controller
    {
        get { return m_Controller; }
    }

    public Rigidbody Rigidbody { get; private set; }

    public PlayerInput PInput { get; private set; }

    public PlayerProperties Properties
    {
        get { return m_PlayerProperties; }
    }

    /// <summary>
    /// Player index
    /// </summary>
    public int Index
    {
        get { return m_PlayerIndex; }
    }
}