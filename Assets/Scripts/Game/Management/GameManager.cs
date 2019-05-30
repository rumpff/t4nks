using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XboxCtrlrInput;

public class GameManager : MonoBehaviour
{
    public static GameManager I;

    private GameObject m_PlayerPrefab;
    private GameObject m_PlayerCameraPrefab;
    private GameObject m_PlayerUIPrefab;

    private GameRules m_GameRules;
    private SpawnLocations m_SpawnLocations;

    public GamePlayer[] Players { get; private set; }

    private void Awake()
    {
        I = this;

        // Load prefabs
        m_PlayerPrefab = Resources.Load("Prefabs/Player") as GameObject;
        m_PlayerCameraPrefab = Resources.Load("Prefabs/PlayerCamera") as GameObject;
        m_PlayerUIPrefab = Resources.Load("Prefabs/UI/PlayerUI") as GameObject;

        // Debug - Create gamerules
        m_GameRules = new GameRules();
    }

    private void Start()
    {
        m_SpawnLocations = GetComponentInChildren<SpawnLocations>();

        InitalizeGame();
    }

    private void InitalizeGame()
    {
        // Debug - create players
        Players = new GamePlayer[]
        {
            new GamePlayer(new PlayerProperties() { Controller = XboxController.First, Name = "jaap", Tank = Resources.Load("Properties/Tanks/TestTank") as TankProperties}, 0),
            new GamePlayer(new PlayerProperties() { Controller = XboxController.Second, Name = "bob", Tank = Resources.Load("Properties/Tanks/TestTank") as TankProperties}, 1),
            new GamePlayer(new PlayerProperties() { Controller = XboxController.Third, Name = "henk", Tank = Resources.Load("Properties/Tanks/TestTank") as TankProperties}, 2),
            //new GamePlayer(new PlayerProperties() { Controller = XboxController.Fourth, Name = "nogeenkeertje", Tank = Resources.Load("Properties/Tanks/TestTank") as TankProperties}, 3)
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
            SpawnPlayer(i);
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

    private void SpawnPlayer(int playerId)
    {
        // Get a location
        Vector3 pos = new Vector3();
        Vector3 rot = new Vector3();

        List<Vector3> playerPositions = new List<Vector3>();

        for (int i = 0; i < Players.Length; i++)
        {
            if (Players[i].Player == null)
                continue;

            playerPositions.Add(Players[i].Player.transform.position);
        }

        pos = m_SpawnLocations.FurthestLocation(playerPositions);
        rot.y = Random.Range(0.0f, 360.0f);

        GameObject pObject = Instantiate(m_PlayerPrefab, pos, Quaternion.Euler(rot));
        pObject.name = "Player " + (Players[playerId].Index + 1);

        Player p = pObject.GetComponent<Player>();

        p.Initalize(Players[playerId].Index, Players[playerId].Properties, Players[playerId].Camera, this);
        p.DestroyedEvent += OnPlayerDeath;
        Players[playerId].Player = p;
        Players[playerId].State = PlayerState.Alive;

    }

    private IEnumerator RespawnPlayer(int playerId)
    {
        Players[playerId].AllowedToRespawn = false;

        yield return new WaitForSeconds(m_GameRules.RespawnDelay);

        Players[playerId].AllowedToRespawn = true;

        // Wait for the player until they want to respawn
        while(!XCI.GetButton(XboxButton.A, Players[playerId].Player.Controller))
        {
            yield return new WaitForEndOfFrame();
        }

        // Spawn the player
        SpawnPlayer(playerId);
    }

    public void OnPlayerDeath(int playerId, Player killer)
    {
        Players[playerId].State = PlayerState.Destroyed;

        if(playerId == killer.Index)
        {
            AddScore(playerId, StatTypes.SelfKill);
        }
        else
        {
            AddScore(killer.Index, StatTypes.Kill);
        }

        StartCoroutine(RespawnPlayer(playerId));
    }

    public void AddScore(int playerId, StatTypes s)
    {
        float scoreAmount = Rules.ScoreValues[s];
        Players[playerId].Properties.Score += scoreAmount;
        AddToStat(playerId, s);

        Players[playerId].UI.NewScore(s.ToString().ToLower(), scoreAmount);
    }

    public void AddScore(int playerId, float scoreOverride, string title)
    {
        Players[playerId].Properties.Score += scoreOverride;
        Players[playerId].UI.NewScore(title, scoreOverride);
    }

    public void AddToStat(int playerId, StatTypes s)
    {
        Players[playerId].Properties.Stats[s]++;
    }

    public void AddToStat(int playerId, StatTypes s, float amount)
    {
        Players[playerId].Properties.Stats[s] += amount;
    }

    public GameRules Rules
    {
        get { return m_GameRules; }
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
                        //return new Rect(0.25f, 0.5f, 0.5f, 0.5f);
                        return new Rect(0f, 0.5f, 1f, 0.5f);

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