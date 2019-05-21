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
    private Camera m_Camera;
    private RectTransform m_RectTransform;

    private RectTransform m_Rectile;

    public void Initalize(int playerIndex, Camera assignedCamera, GameManager gameManger)
    {
        m_GameManager = gameManger;
        m_Camera = assignedCamera;
        m_PlayerIndex = playerIndex;

        m_RectTransform = GetComponent<RectTransform>();
        m_Canvas = GetComponent<Canvas>();

        m_Canvas.renderMode = RenderMode.ScreenSpaceCamera;
        m_Canvas.worldCamera = m_Camera;

        // Find children
        m_Rectile = transform.Find("Rectile") as RectTransform;
    }

    private void Update()
    {
        CalculateRectilePosition();
    }

    private void CalculateRectilePosition()
    {
        Vector3 physicalPosition = m_Player.Weapon.AimPosition();
        Vector3 viewportPosition = m_Player.Camera.Camera.WorldToViewportPoint(physicalPosition);

        // Prevent the rectile from coming back when the position is behind the camera
        viewportPosition *= Mathf.Sign(viewportPosition.z);     

        Vector2 canvasPosition = new Vector2(
         ((viewportPosition.x * m_RectTransform.sizeDelta.x) - (m_RectTransform.sizeDelta.x * 0.5f)),
         ((viewportPosition.y * m_RectTransform.sizeDelta.y) - (m_RectTransform.sizeDelta.y * 0.5f)));

        m_Rectile.anchoredPosition = canvasPosition;
    }
}
