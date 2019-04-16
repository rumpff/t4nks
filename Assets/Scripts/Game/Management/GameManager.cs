using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private GameObject m_PlayerPrefab;

    private void Awake()
    {
        m_PlayerPrefab = Resources.Load("Prefabs/Player") as GameObject;
    }
}
