using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using SimpleEasing;

public class RespawnUI : MonoBehaviour
{
    private RectTransform m_Rect;

    private int m_PlayerIndex;
    private GameManager m_GameManager;

    [SerializeField] private TextMeshProUGUI m_RespawnTimer;

    private bool m_IsActive;

    private float m_HideY;
    private float m_HideTimer;

    public void Initalize(int playerIndex, GameManager gameManager)
    {
        m_Rect = GetComponent<RectTransform>();

        m_PlayerIndex = playerIndex;
        m_GameManager = gameManager;

        m_IsActive = false;

        m_HideY = -m_Rect.sizeDelta.y;
    }

    private void Update()
    {
        CheckState();

        if (m_IsActive)
            ActiveUpdate();
        else
            InactiveUpdate();
    }

    private void CheckState()
    {
        m_IsActive = (m_GameManager.Players[m_PlayerIndex].State == PlayerState.Destroyed);
    }

    private void ActiveUpdate()
    {
        // Position
        m_HideTimer += Time.deltaTime;
        float y = Easing.Ease(EaseType.EaseOutElastic, Mathf.Clamp01(m_HideTimer), m_HideY, -m_HideY, 1);
        m_Rect.anchoredPosition = new Vector2(0, y);

        // Timer
        float respawnTime = m_GameManager.Players[m_PlayerIndex].RespawnTimer;
        string respawnText = respawnTime.ToString("000.000");

        if (respawnTime <= 0)
            respawnText = "press [jump] to respawn!";

        m_RespawnTimer.text = respawnText;

    }

    private void InactiveUpdate()
    {
        m_HideTimer = 0;
        m_Rect.anchoredPosition = new Vector2(0, m_HideY);
    }
}
