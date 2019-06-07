using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XboxCtrlrInput;

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
    private WheelCollider m_WheelFrontLeft, m_WheelFrontRight, m_WheelBackLeft, m_WheelBackRight;
    private List<WheelHit> m_WheelHits;

    private Vector2 m_AimRotation;

    private float m_Torque = 0;
    private float m_BrakeTorque = 0;
    private float m_StreerAngle = 0;

    private float m_JumpTimer = 0;
    private int m_JumpsLeft = 0;

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

        StartCoroutine(ReadWheelHits());

        // Set the layer masks
        MeshRenderer[] meshObjects = transform.GetComponentsInChildren<MeshRenderer>();

        for (int i = 0; i < meshObjects.Length; i++)
        {
            if(meshObjects[i].transform.name.EndsWith("[TPOnly]"))
            {
                // Set the Third person only layer masks
                meshObjects[i].gameObject.layer = 11 + playerIndex;
            }
        }
    }

    private void Update()
    {
        ReadWheelHits();

        CarThing();
        TimerThing();
    }

    private void FixedUpdate()
    {
        ApplyCarMotion();
        AircontrolThing();
        JumpThing();
    }

    private void CarThing()
    {
        // Handle torque
        float torqueDest = 0;

        if(IsOnGround())
            torqueDest = PInput.Torque * m_TankProperties.MaxTorque;

        if (m_Torque != torqueDest)
        {
            float changeDir = Mathf.Sign(torqueDest - m_Torque);
            m_Torque += changeDir * (m_TankProperties.AccelerateRate * Time.deltaTime);

            // Prevent overshooting
            if(changeDir == 1 && (m_Torque > torqueDest) ||
                changeDir == -1 && (m_Torque < torqueDest))
            {
                m_Torque = torqueDest;
            }
        }

        // Handle steer
        m_StreerAngle = Mathf.Lerp(m_StreerAngle, PInput.Steer * m_TankProperties.MaxSteer, 12 * Time.deltaTime);
        
        // Handle brake
        m_BrakeTorque = (PInput.Brake) ? m_TankProperties.BrakeTorque : 0;
    }

    private void AircontrolThing()
    {
        if (IsOnGround())
            return; // Not in the air

        Rigidbody.AddTorque(transform.up * (PInput.AirControl.x * m_TankProperties.AirControl));
        Rigidbody.AddTorque(-transform.right * (PInput.AirControl.y * m_TankProperties.AirControl));
    }

    private void JumpThing()
    {
        if (PInput.Jump && m_JumpTimer <= 0)
        {
            if (m_JumpsLeft <= 0)
                return;

            Vector3 jumpDirection = GetGroundNormal(transform.up);

            Rigidbody.AddForce(jumpDirection * m_TankProperties.JumpForce);

            m_JumpsLeft--;
            m_JumpTimer = JumpCooldown;
        }
    }

    private void TimerThing()
    {
        // Jumping
        if (IsOnGround())
            m_JumpsLeft = m_TankProperties.AirJumpAmount;
        m_JumpTimer -= Time.deltaTime;
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

    /// <summary>
    /// Absolute aim rotation
    /// </summary>
    public Vector2 RawAim
    {
        get { return new Vector2(m_AimRotation.x / MaxAim.x, m_AimRotation.y / MaxAim.y); }
    }

    public XboxController Controller
    {
        get { return m_Controller; }
    }

    public Rigidbody Rigidbody { get; private set; }

    public PlayerInput PInput { get; private set; }

    public int Index
    {
        get { return m_PlayerIndex; }
    }
}