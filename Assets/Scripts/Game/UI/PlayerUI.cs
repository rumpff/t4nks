using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerUI : MonoBehaviour
{
    private int m_PlayerIndex;
    private GameManager m_GameManager;
    private Canvas m_Canvas;
    private RectTransform m_RectTransform;

    [SerializeField] private RectTransform m_Rectile;
    [Space(5)]
    [SerializeField] private Image m_DamageOverlay;
    [SerializeField] private Image  m_Healthbar;
    [Space(5)]
    [SerializeField] private TextMeshProUGUI m_ScoreText;

    public void Initalize(int playerIndex, GameManager gameManger)
    {
        m_GameManager = gameManger;
        m_PlayerIndex = playerIndex;

        m_RectTransform = GetComponent<RectTransform>();
        m_Canvas = GetComponent<Canvas>();

        m_Canvas.renderMode = RenderMode.ScreenSpaceCamera;
        m_Canvas.worldCamera = m_GameManager.Players[m_PlayerIndex].Camera.Camera;
        m_Canvas.planeDistance = 0.15f;

        SetDamageOverlay(0.0f);
    }

    private void Update()
    {
        CalculateRectilePosition();
        UpdateDamageOverlay();
        UpdateHealthbar();
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

    private void SetDamageOverlay(float alpha)
    {
        Color c = m_DamageOverlay.color;
        c.a = alpha;

        m_DamageOverlay.color = c;
    }

    private void UpdateDamageOverlay()
    {
        Color c = m_DamageOverlay.color;
        c.a = Mathf.Lerp(c.a, 0, 6 * Time.deltaTime);

        m_DamageOverlay.color = c;
    }

    private void UpdateHealthbar()
    {
        float f = m_GameManager.Players[m_PlayerIndex].Player.Health.Health / m_GameManager.Players[m_PlayerIndex].Player.Health.MaxHealth;
        float a = m_Healthbar.fillAmount;

        a = Mathf.Lerp(a, f, 12 * Time.deltaTime);

        m_Healthbar.fillAmount = a;
    }

    public void AddDamageEffect(float damageAmount, float maxHealth)
    {
        float p = damageAmount / (maxHealth / 2);

        Color c = m_DamageOverlay.color;
        c.a += p;

        m_DamageOverlay.color = c;
    }
}
