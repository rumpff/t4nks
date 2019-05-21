using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private GameObject m_PlayerPrefab;
    private GameObject m_PlayerCameraPrefab;
    private GameObject m_PlayerUIPrefab;

    public GamePlayer[] Players { get; private set; }

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
        Players = new GamePlayer[]
        {
            new GamePlayer(new PlayerProperties() { Controller = XboxCtrlrInput.XboxController.First, Name = "jaap", Tank = Resources.Load("Properties/Tanks/TestTank") as TankProperties}, 0),
            new GamePlayer(new PlayerProperties() { Controller = XboxCtrlrInput.XboxController.Second, Name = "bob", Tank = Resources.Load("Properties/Tanks/TestTank") as TankProperties}, 1),
            new GamePlayer(new PlayerProperties() { Controller = XboxCtrlrInput.XboxController.First, Name = "jaap", Tank = Resources.Load("Properties/Tanks/TestTank") as TankProperties}, 2),
            new GamePlayer(new PlayerProperties() { Controller = XboxCtrlrInput.XboxController.Second, Name = "bob", Tank = Resources.Load("Properties/Tanks/TestTank") as TankProperties}, 3)
        };

        // Create cameras
        CameraProperties defaultCameraProperty = Resources.Load("Properties/Camera/DefaultCamera") as CameraProperties;
        for (int i = 0; i < Players.Length; i++)
        {
            // Spawn camra
            GameObject cObject = Instantiate(m_PlayerCameraPrefab);
            cObject.name = "PlayerCamera (" + (i + 1) + ")";

            PlayerCamera pCamera = cObject.GetComponent<PlayerCamera>();
            pCamera.Initalize(i, defaultCameraProperty, CalculateCameraRect(i, Players.Length), this);

            Players[i].Camera = pCamera;
        }

        // Create players
        for (int i = 0; i < Players.Length; i++)
        {
            SpawnPlayer(ref Players[i]);
        }

        // Create UIs
        for (int i = 0; i < Players.Length; i++)
        {
            // Spawn UI
            GameObject uObject = Instantiate(m_PlayerUIPrefab);
            uObject.name = "PlayerUI (" + (i + 1) + ")";

            PlayerUI pUI = uObject.GetComponent<PlayerUI>();
            pUI.Initalize(i, this);

            Players[i].UI = pUI;
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
        pObject.name = "Player " + (player.Index + 1);

        Player p = pObject.GetComponent<Player>();

        p.Initalize(player.Properties, player.Camera);
        player.Player = p;
    }

    private Rect CalculateCameraRect(int index, int cameraAmount)
    {
        switch(cameraAmount)
        {
            case 2:
                switch (index + 1)
                {
                    case 1:
                        return new Rect(0.0f, 0.5f, 1.0f, 0.5f);

                    case 2:
                        return new Rect(0.0f, 0.0f, 1.0f, 0.5f);
                }
                break;

            case 3:
                switch (index + 1)
                {
                    case 1:
                        return new Rect(0.0f, 0.5f, 1.0f, 0.5f);

                    case 2:
                        return new Rect(0.0f, 0.0f, 0.5f, 0.5f);

                    case 3:
                        return new Rect(0.5f, 0.0f, 0.5f, 0.5f);
                }
                break;

            case 4:
                switch (index + 1)
                {
                    case 1:
                        return new Rect(0.0f, 0.5f, 0.5f, 0.5f);

                    case 2:
                        return new Rect(0.5f, 0.5f, 0.5f, 0.5f);

                    case 3:
                        return new Rect(0.0f, 0.0f, 0.5f, 0.5f);

                    case 4:
                        return new Rect(0.5f, 0.0f, 0.5f, 0.5f);
                }
                break;
        }

        return new Rect(0, 0, 1, 1);

        #region i tried
        /*
        float x;
        float y;
        float w;
        float h;

        y = 0.5f * (index > cameraAmount ? 1 : 0);
        h = 0.5f;

        int halveAmount = cameraAmount / 2;
        int halveIndex = index;
        bool isEven = (cameraAmount % 2 == 0);


        if(y == 0.5f) // Port is on the lower half
        {
            // If we're on the bottom remove the amount of screens on top off our index
            halveIndex -= halveAmount;
            if (!isEven)
            {
                halveAmount++;
            }
        }

         x = (halveIndex / halveAmount);
         w = 1.0f / halveAmount;


        Rect rect = new Rect(x, y, w, h);
        return rect;
        */
        #endregion
    }
}

public struct GamePlayer
{
    public PlayerProperties Properties;

    public Player Player;
    public PlayerState State;
    public PlayerCamera Camera;
    public PlayerUI UI;
    public int Index;

    public GamePlayer(PlayerProperties properties, int index)
    {
        Properties = properties;
        State = PlayerState.Alive;
        Index = index;

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