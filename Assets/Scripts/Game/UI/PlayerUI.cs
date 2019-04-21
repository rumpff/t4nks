using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerUI : MonoBehaviour
{
    [SerializeField]
    private Player m_Player;
    private Canvas m_Canvas;
    private RectTransform m_Rect;

    [SerializeField]
    private RectTransform TESTEBOI;

    private void Start()
    {
        m_Rect = GetComponent<RectTransform>();
        m_Canvas = GetComponent<Canvas>();
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
         ((viewportPosition.x * m_Rect.sizeDelta.x) - (m_Rect.sizeDelta.x * 0.5f)),
         ((viewportPosition.y * m_Rect.sizeDelta.y) - (m_Rect.sizeDelta.y * 0.5f)));

        TESTEBOI.anchoredPosition = canvasPosition;
    }
}
