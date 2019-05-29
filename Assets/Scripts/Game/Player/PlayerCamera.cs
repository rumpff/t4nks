using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimpleEasing;

public class PlayerCamera : MonoBehaviour
{
    public Vector3 floot;

    public enum CameraStates { following, zoomed }
    private Dictionary<CameraStates, CameraValues> m_CameraValues;
    private float m_CameraState = 0;

    private Camera m_Camera;
    private CameraProperties m_CameraProperties;
    private GameManager m_GameManager;
    private int m_PlayerIndex;

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

    // The player's thingz
    private Vector3 m_PlayerPos, m_PlayerHeadPos;

    // Screenshake
    private readonly float m_ScreenshakeMaxPosition = 2;
    private readonly float m_ScreenshakeMaxRotation = 20;

    private float m_ScreenshakeAmount;
    private Vector3 m_ScreenshakePosition;
    private Vector3 m_ScreenshakeRotation;

    public void Initalize(int playerIndex, CameraProperties cameraProperties, Rect viewport, GameManager gameManager)
    {
        m_PlayerIndex = playerIndex;
        m_CameraProperties = cameraProperties;
        m_GameManager = gameManager;

        m_Camera = GetComponent<Camera>();
        m_Camera.rect = viewport;

        m_CameraValues = new Dictionary<CameraStates, CameraValues>();
        m_CameraValues.Add(CameraStates.following, new CameraValues(90.0f));
        m_CameraValues.Add(CameraStates.zoomed, new CameraValues(55.0f));

        StartCoroutine(CameraUpdate());
    }

    private void Update()
    {
        int state = XboxCtrlrInput.XCI.GetAxis(XboxCtrlrInput.XboxAxis.LeftTrigger, XboxCtrlrInput.XboxController.First) > 0.4f ? 1 : -1;
        m_CameraState += state * Time.deltaTime * 1.5f;
        m_CameraState = Mathf.Clamp01(m_CameraState);

        UpdatePostEffects();
    }

    private IEnumerator CameraUpdate()
    {
        while(true)
        {
            yield return new WaitForFixedUpdate();

            UpdateVariables();

            UpdateFollowState();
            UpdateZoomedState();

            // Apply values
            float t = Easing.easeInOutQuint(m_CameraState, 0, 1, 1);

            transform.position = CameraValues.Lerp(m_CameraValues[CameraStates.following], m_CameraValues[CameraStates.zoomed], t).Position;
            transform.rotation = CameraValues.Lerp(m_CameraValues[CameraStates.following], m_CameraValues[CameraStates.zoomed], t).Rotation;
            m_Camera.fieldOfView = CameraValues.Lerp(m_CameraValues[CameraStates.following], m_CameraValues[CameraStates.zoomed], t).FOV;

            // Apply post effects
            transform.position += m_BumpOffset;

            transform.position += m_ScreenshakePosition;
            transform.rotation = Quaternion.Euler(transform.eulerAngles + m_ScreenshakeRotation);           
        }
    }

    private void UpdateFollowState()
    {
        CameraValues v = m_CameraValues[CameraStates.following];

        // Lerp the rotation
        m_ViewAngle = Mathf.LerpAngle(m_ViewAngle, m_angleDest + m_AimOffset.x, LerpInterpolation);
        m_HeightAngle = Mathf.LerpAngle(m_HeightAngle, m_BaseHeight + m_AimOffset.y, LerpInterpolation);
        m_Distance = Mathf.Lerp(m_Distance, m_BaseDistance, LerpInterpolation);

        // Calculate the final position
        v.Position.x = m_PlayerPos.x + Mathf.Sin((m_ViewAngle - 180) * Mathf.Deg2Rad) * (Mathf.Cos(m_HeightAngle * Mathf.Deg2Rad) * m_Distance);
        v.Position.y = m_PlayerPos.y + Mathf.Sin(m_HeightAngle * Mathf.Deg2Rad) * m_Distance;
        v.Position.z = m_PlayerPos.z + Mathf.Cos((m_ViewAngle - 180) * Mathf.Deg2Rad) * (Mathf.Cos(m_HeightAngle * Mathf.Deg2Rad) * m_Distance);

        if (m_GameManager.Players[m_PlayerIndex].Player != null)
            v.Rotation = Quaternion.LookRotation(m_GameManager.Players[m_PlayerIndex].Player.transform.position - transform.position);

        m_CameraValues[CameraStates.following] = v;
    }

    private void UpdateZoomedState()
    {
        CameraValues v = m_CameraValues[CameraStates.zoomed];

        // Calculate the final position
        v.Position = m_PlayerHeadPos;

        if (m_GameManager.Players[m_PlayerIndex].Player != null)
            v.Rotation = Quaternion.LookRotation(m_GameManager.Players[m_PlayerIndex].Player.Weapon.BarrelEnd.forward);

        m_CameraValues[CameraStates.zoomed] = v;
    }

    private void UpdatePostEffects()
    {
        // Shoot bump
        m_BumpOffset = Vector3.Lerp(m_BumpOffset, Vector3.zero, LerpInterpolation);

        // Screenshake
        m_ScreenshakePosition.x = (Mathf.PerlinNoise(Time.time * 4.5f, 3) * m_ScreenshakeMaxPosition) * m_ScreenshakeAmount;
        m_ScreenshakePosition.y = (Mathf.PerlinNoise(Time.time * 4.5f, 4) * m_ScreenshakeMaxPosition) * m_ScreenshakeAmount;
        m_ScreenshakePosition.z = (Mathf.PerlinNoise(Time.time * 4.5f, 5) * m_ScreenshakeMaxPosition) * m_ScreenshakeAmount;

        m_ScreenshakeRotation.x = (Mathf.PerlinNoise(Time.time * 3.5f, 6) * m_ScreenshakeMaxRotation) * m_ScreenshakeAmount;
        m_ScreenshakeRotation.y = (Mathf.PerlinNoise(Time.time * 3.5f, 7) * m_ScreenshakeMaxRotation) * m_ScreenshakeAmount;
        m_ScreenshakeRotation.z = (Mathf.PerlinNoise(Time.time * 3.5f, 8) * m_ScreenshakeMaxRotation) * m_ScreenshakeAmount;

        m_ScreenshakeAmount = Mathf.Lerp(m_ScreenshakeAmount, 0, 3 * Time.deltaTime);
    }

    private void UpdateVariables()
    {
        if (m_GameManager.Players[m_PlayerIndex].Player == null)
            return;

        m_PlayerPos = m_GameManager.Players[m_PlayerIndex].Player.transform.position;

        m_PlayerHeadPos = m_GameManager.Players[m_PlayerIndex].Player.Weapon.BarrelBegin;

        // Only update the angle when the player is on the ground
        if (m_GameManager.Players[m_PlayerIndex].Player.IsOnGround())
            m_angleDest = m_GameManager.Players[m_PlayerIndex].Player.transform.eulerAngles.y;

        m_AimOffset = new Vector2(
            m_GameManager.Players[m_PlayerIndex].Player.Weapon.RawAim.x * 45.0f,
            m_GameManager.Players[m_PlayerIndex].Player.Weapon.RawAim.y * 20); // 15.0f
    }

    public void AddScreenshake(float amount)
    {
        m_ScreenshakeAmount += amount;
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

public struct CameraValues
{
    public Vector3 Position;
    public Quaternion Rotation;
    public float FOV;

    public CameraValues(float fov)
    {
        Position = new Vector3();
        Rotation = new Quaternion();
        FOV = fov;
    }

    public static CameraValues Lerp(CameraValues a, CameraValues b, float t)
    {
        CameraValues o = new CameraValues();

        o.Position = Vector3.Lerp(a.Position, b.Position, t);
        o.Rotation = Quaternion.Lerp(a.Rotation, b.Rotation, t);
        o.FOV = Mathf.Lerp(a.FOV, b.FOV, t);

        return o;
    }
}