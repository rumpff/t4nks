using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private GameObject m_PlayerPrefab;
    private GameObject m_PlayerCameraPrefab;
    private GameObject m_PlayerUIPrefab;

    private GamePlayer[] m_Players;

    private void Awake()
    {
        // Load prefabs
        m_PlayerPrefab = Resources.Load("Prefabs/Player") as GameObject;
        m_PlayerCameraPrefab = Resources.Load("Prefabs/PlayerCamera") as GameObject;
        m_PlayerUIPrefab = Resources.Load("Prefabs/PlayerUI") as GameObject;
    }

    private void Start()
    {
        InitalizeGame();
    }

    private void InitalizeGame()
    {
        // Debug filling this rn
        m_Players = new GamePlayer[]
        {
            new GamePlayer(new PlayerProperties() { Controller = XboxCtrlrInput.XboxController.First, Name = "jaap", Tank = Resources.Load("Properties/Tanks/TestTank") as TankProperties}),
            new GamePlayer(new PlayerProperties() { Controller = XboxCtrlrInput.XboxController.Second, Name = "bob", Tank = Resources.Load("Properties/Tanks/TestTank") as TankProperties})
        };

        // Create cameras
        CameraProperties defaultCameraProperty = Resources.Load("Properties/Camera/DefaultCamera") as CameraProperties;
        for (int i = 0; i < m_Players.Length; i++)
        {
            // Spawn camra
            GameObject cObject = Instantiate(m_PlayerCameraPrefab);
            cObject.name = "PlayerCamera (" + (i + 1) + ")";

            PlayerCamera pCamera = cObject.GetComponent<PlayerCamera>();
            pCamera.Initalize(i, defaultCameraProperty, this);

            m_Players[i].Camera = pCamera;
        }

        // Create players
        for (int i = 0; i < m_Players.Length; i++)
        {
            SpawnPlayer(ref m_Players[i]);
        }

        // Create UIs
        for (int i = 0; i < m_Players.Length; i++)
        {
            // Spawn UI
            GameObject uObject = Instantiate(m_PlayerUIPrefab);
            uObject.name = "PlayerUI (" + (i + 1) + ")";

            PlayerUI pUI = uObject.GetComponent<PlayerUI>();
            pUI.Initalize(i, m_Players[i].Camera.Camera, this);

            m_Players[i].UI = pUI;
        }
    }

    private void SpawnPlayer(ref GamePlayer player)
    {
        // For now create a random spawn position
        Vector3 pos = new Vector3
        {
            x = Random.Range(-220.0f, 220.0f),
            y = 10.0f,
            z = Random.Range(-220.0f, 220.0f)
        };

        GameObject pObject = Instantiate(m_PlayerPrefab, pos, Quaternion.Euler(Vector3.zero));
        Player p = pObject.GetComponent<Player>();

        p.Initalize(player.Properties, player.Camera);
        player.Player = p;
    }
}

public struct GamePlayer
{
    public PlayerProperties Properties;

    public Player Player;
    public PlayerState State;
    public PlayerCamera Camera;
    public PlayerUI UI;

    public GamePlayer(PlayerProperties properties)
    {
        Properties = properties;
        State = PlayerState.Alive;

        // These values will be assigned once they're created
        Player = null;
        Camera = null;
        UI = null;
    }
}

public enum PlayerState
{
    Alive, Destroyed
};