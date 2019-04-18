using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    private bool m_HasExploded = false;
    private float m_BurnoutTime;
    private Rigidbody m_Rigidbody;
    private WeaponProperties m_WeaponProperties;

    private void Awake()
    {
        m_Rigidbody = GetComponent<Rigidbody>();
    }

    public void Initalize(WeaponProperties weaponInfo, Vector3 aimDirection)
    {
        m_WeaponProperties = weaponInfo;

        m_Rigidbody.mass = weaponInfo.Mass;
        m_Rigidbody.AddForce(aimDirection * weaponInfo.InitialForce);
    }

	private void OnCollisionEnter(Collision collision)
	{
        if (m_HasExploded)
            return;

        // Create explosion
        Explosion e = Instantiate(m_WeaponProperties.Explosion, transform.position, Quaternion.Euler(0, 0, 0));
        e.Initalize(m_WeaponProperties.ExplosionProperties);

        StartCoroutine(ProjectileBurnout());
        m_HasExploded = true;
    }

    private IEnumerator ProjectileBurnout()
    {
        // Disable the visible projectile
        GetComponent<MeshRenderer>().enabled = false;

        // Stop all particle systems
        ParticleSystem[] pSystems = GetComponentsInChildren<ParticleSystem>();

        foreach(ParticleSystem p in pSystems)
        {
            ParticleSystem.EmissionModule e = p.emission;
            e.rateOverTime = 0;
        }

        while (m_BurnoutTime < m_WeaponProperties.BurnoutTime)
        {
            m_BurnoutTime += Time.deltaTime;
            yield return new WaitForEndOfFrame();

            // Force the rigidbody to sleep
            m_Rigidbody.Sleep();
        }

        Destroy(gameObject);
    }
}
