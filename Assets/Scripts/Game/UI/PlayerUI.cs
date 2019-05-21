using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerUI : MonoBehaviour
{
    [SerializeField]
    private Player m_Player;
    private int m_PlayerIndex;
    private GameManager m_GameManager;
    private Canvas m_Canvas;
    private RectTransform m_RectTransform;

    private RectTransform m_Rectile;

    public void Initalize(int playerIndex, GameManager gameManger)
    {
        m_GameManager = gameManger;
        m_PlayerIndex = playerIndex;

        m_RectTransform = GetComponent<RectTransform>();
        m_Canvas = GetComponent<Canvas>();

        m_Canvas.renderMode = RenderMode.ScreenSpaceCamera;
        m_Canvas.worldCamera = m_GameManager.Players[m_PlayerIndex].Camera.Camera;

        // Find children
        m_Rectile = transform.Find("Rectile") as RectTransform;
    }

    private void Update()
    {
        CalculateRectilePosition();
    }

    private void CalculateRectilePosition()
    {
        if (m_GameManager.Players[m_PlayerIndex].Player == null)
            return;

        Vector3 physicalPosition = m_GameManager.Players[m_PlayerIndex].Player.Weapon.AimPosition();
        Vector3 viewportPosition = m_GameManager.Players[m_PlayerIndex].Camera.Camera.WorldToViewportPoint(physicalPosition);

        // Prevent the rectile from coming back when the position is behind the camera
        viewportPosition *= Mathf.Sign(viewportPosition.z);     

        Vector2 canvasPosition = new Vector2(
         ((viewportPosition.x * m_RectTransform.sizeDelta.x) - (m_RectTransform.sizeDelta.x * 0.5f)),
         ((viewportPosition.y * m_RectTransform.sizeDelta.y) - (m_RectTransform.sizeDelta.y * 0.5f)));

        m_Rectile.anchoredPosition = canvasPosition;
    }
}
