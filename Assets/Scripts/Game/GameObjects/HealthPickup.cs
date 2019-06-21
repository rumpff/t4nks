using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthPickup : MonoBehaviour
{
    private bool m_PickupActive;

    [SerializeField]
    private GameObject m_HealthBlock;

    [SerializeField]
    private Explosion m_HealthExplosion;
    [SerializeField]
    private ExplosionProperties m_HealthExplosionProperties;

    [SerializeField]
    private float m_BaseHeight, m_WaveHeight, m_WaveRotation, m_HeightSpeed, m_RotateSpeed, m_HeightMultiplier;

    private void Start()
    {
        m_HeightMultiplier = 0;
        m_PickupActive = true;
    }

    void Update()
    {
        // Move the health block
        if (m_PickupActive)
        {
            m_HealthBlock.transform.localPosition = new Vector3()
            {
                x = 0,
                y = (m_BaseHeight + (Mathf.Sin(Time.time * m_HeightSpeed) * m_WaveHeight)) * m_HeightMultiplier,
                z = 0
            };

            m_HealthBlock.transform.localEulerAngles = new Vector3()
            {
                x = Mathf.Sin(Time.time) * m_WaveRotation,
                y = Time.time * m_RotateSpeed,
                z = 0
            };

            m_HealthBlock.transform.localScale = Vector3.one;

            m_HeightMultiplier = Mathf.Lerp(m_HeightMultiplier, 1, 15 * Time.deltaTime);
        }
    }

    public void HealthPickedUp(Player p)
    {
        StartCoroutine(HealthPickupBehaviour(p));
    }
    private IEnumerator HealthPickupBehaviour(Player p)
    {
        if(!m_PickupActive)
            yield break;

        m_PickupActive = false;
        float suckTimer = 0.3f;

        while(suckTimer > 0)
        {
            Vector3 bRotate = m_HealthBlock.transform.localEulerAngles;
            Vector3 bDest = bRotate; bDest.z = 90.0f;

            if(p != null)
            {
                m_HealthBlock.transform.position = Vector3.Lerp(m_HealthBlock.transform.position, p.transform.position, 9 * Time.fixedDeltaTime);
                m_HealthBlock.transform.localScale = Vector3.Lerp(m_HealthBlock.transform.localScale, Vector3.zero, 9 * Time.fixedDeltaTime);
                m_HealthBlock.transform.localEulerAngles = Vector3.Lerp(bRotate, bDest, 14 * Time.fixedDeltaTime);
            }

            suckTimer -= Time.deltaTime;
            yield return new WaitForFixedUpdate();
        }

        // Add health to the player
        if (p != null)
            p.Health.AddHealth(GameManager.I.Rules.HealthPickupHealthAmount);

        // Handle the pickup
        m_PickupActive = false;
        m_HealthBlock.SetActive(false);

        Explosion e = Instantiate(m_HealthExplosion, m_HealthBlock.transform.position, Quaternion.Euler(0, 0, 0));
        e.Initalize(p, m_HealthExplosionProperties, new HitProperties(false));

        yield return new WaitForSeconds(GameManager.I.Rules.HealthPickupRespawnTime);

        m_HeightMultiplier = 0;

        m_PickupActive = true;
        m_HealthBlock.SetActive(true);
    }
}
