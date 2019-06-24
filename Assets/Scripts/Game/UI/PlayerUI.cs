using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SimpleEasing;

public class PlayerUI : MonoBehaviour
{
    private int m_PlayerIndex;
    private GameManager m_GameManager;
    private Canvas m_Canvas;
    private RectTransform m_RectTransform;

    private Color m_TextOutlineColor;

    private GameObject m_ScorePopupPrefab;

    private List<ScorePopupProperties> m_PopupQueue;
    private float m_DisplayScore = 0;
    private int m_PopupsActive = 0;

    [SerializeField] private RectTransform m_Rectile;
    [Space(5)]
    [SerializeField] private Image m_DamageOverlay;
    [SerializeField] private Image  m_Healthbar;
    [SerializeField] private Image m_CurrentWeapon;
    [Space(5)]
    [SerializeField] private TextMeshProUGUI m_HealthText;
    [SerializeField] private TextMeshProUGUI m_AmmoText;

    public void Initalize(int playerIndex, GameManager gameManger)
    {
        m_GameManager = gameManger;
        m_PlayerIndex = playerIndex;

        m_RectTransform = GetComponent<RectTransform>();
        m_Canvas = GetComponent<Canvas>();

        m_Canvas.renderMode = RenderMode.ScreenSpaceCamera;
        m_Canvas.worldCamera = m_GameManager.Players[m_PlayerIndex].Camera.Camera;
        m_Canvas.planeDistance = 0.15f;

        m_ScorePopupPrefab = Resources.Load("Prefabs/UI/ScorePopup") as GameObject;
        m_PopupQueue = new List<ScorePopupProperties>();

        m_TextOutlineColor = Utill.PlayerColor(m_PlayerIndex);
        m_TextOutlineColor.a = 0.5f;

        // Personalize the font styles
        m_HealthText.fontMaterial.SetColor("_OutlineColor", m_TextOutlineColor);

        SetDamageOverlay(0.0f);
    }

    private void Update()
    {
        CalculateRectilePosition();
        UpdateDamageOverlay();
        UpdateHealthbar();
        UpdateWeaponbar();
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
        float health = m_GameManager.Players[m_PlayerIndex].Player.Health.Health;
        float maxHealth = m_GameManager.Players[m_PlayerIndex].Player.Health.MaxHealth;
        float f = health / maxHealth;
        float a = m_Healthbar.fillAmount;

        a = Mathf.Lerp(a, f, 12 * Time.deltaTime);

        m_Healthbar.fillAmount = a;
        m_HealthText.text = (a * maxHealth).ToString("000");
    }

    private void UpdateWeaponbar()
    {
        float ammo = m_GameManager.Players[m_PlayerIndex].Player.Weapon.Ammo;
        string ammoString = ammo.ToString("000");

        if (ammo == 0)
            ammoString = "inf";

        m_AmmoText.text = ammoString;
    }

    public void NewScore(string title, float amount)
    {
        // Poptup for a score of 0 looks wierd
        if ((int)amount == 0)
            return;
     
        StartCoroutine(QueuePopups(title, amount));
    }

    private IEnumerator QueuePopups(string title, float amount)
    {
        m_PopupQueue.Add(new ScorePopupProperties(title, amount));
        yield return new WaitForEndOfFrame();

        if(m_PopupQueue.Count > 0)
        {
            string finalTitle = string.Empty; ;
            float finalAmount = 0;

            for (int i = 0; i < m_PopupQueue.Count; i++)
            {
                finalTitle += m_PopupQueue[i].Title;
                finalAmount += m_PopupQueue[i].ScoreAmount;

                if (i != (m_PopupQueue.Count - 1))
                    finalTitle += ", ";
            }

            m_PopupsActive++;
            StartCoroutine(ScorePopup(finalTitle, finalAmount));
            m_PopupQueue = new List<ScorePopupProperties>();
        }
    }

    private IEnumerator ScorePopup(string title, float amount)
    {
        float y = (m_PopupsActive-1) * 70.0f;
        float score = amount;
        float subtractAmount = score / 40.0f;
        string operatorType = amount >= 0 ? "+" : "";

        float inTimer = 0;
        float inLength = 1.5f;
        float outTimer = 0;
        float outLength = 0.6f;

        TextMeshProUGUI sP = Instantiate(m_ScorePopupPrefab, m_Canvas.transform).GetComponent<TextMeshProUGUI>();
        sP.rectTransform.anchoredPosition = new Vector2(0, y);
        sP.text = "(" + title + ") " + operatorType + amount.ToString("0");

        // Personalize the font styles
        sP.fontMaterial.SetColor("_OutlineColor", m_TextOutlineColor);

        while (inTimer < inLength)
        {
            inTimer += Time.deltaTime;

            if (inTimer > inLength)
                inTimer = inLength;

            float scale = Easing.easeOutElastic(inTimer, 0, 1, inLength);
            sP.rectTransform.localScale = new Vector3(scale, scale, 1);

            yield return new WaitForEndOfFrame();
        }

        while(LargerIfPositive(score, 0))
        {
            if (SmallerIfPositive(score, subtractAmount))
            {
                score -= score;
                m_DisplayScore += score;
            }
            else
            {
                score -= subtractAmount;
                m_DisplayScore += subtractAmount;
            }


            sP.text = "(" + title + ") " + operatorType + score.ToString("0");
            yield return new WaitForEndOfFrame();
        }

        while (outTimer < outLength)
        {
            outTimer += Time.deltaTime;

            if (outTimer > outLength)
                outTimer = outLength;

            float scale = Easing.easeInExpo(outTimer, 1, -1, outLength);
            float alpha = Easing.easeInExpo(outTimer, 1, -1, outLength);
            float angle = Easing.easeInExpo(outTimer, 0, -30, outLength);

            sP.rectTransform.localScale = new Vector3(scale, scale, 1);
            sP.rectTransform.localEulerAngles = new Vector3(0, 0, angle);
            sP.color = new Color(sP.color.r, sP.color.g, sP.color.b, alpha);

            yield return new WaitForEndOfFrame();
        }

        Destroy(sP);
        m_PopupsActive--;
    }

    public void AddDamageEffect(float damageAmount, float maxHealth)
    {
        float p = damageAmount / (maxHealth / 2);

        Color c = m_DamageOverlay.color;
        c.a += p;

        m_DamageOverlay.color = c;
    }

    // I cant think of names for these fucntions
    private bool LargerIfPositive(float value, float comparer)
    {
        if (value >= 0)
            return (value > comparer);
        else
            return (value < comparer);
    }
    private bool SmallerIfPositive(float value, float comparer)
    {
        if (value >= 0)
            return (value < comparer);
        else
            return (value > comparer);
    }
}

public struct ScorePopupProperties
{
    public string Title;
    public float ScoreAmount;

    public ScorePopupProperties(string t, float s)
    {
        Title = t;
        ScoreAmount = s;
    }
}
