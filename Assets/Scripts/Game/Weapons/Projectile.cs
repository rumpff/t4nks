using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    private Player m_Owner;
    private bool m_HasExploded = false;
    private float m_BurnoutTime;
    private Rigidbody m_Rigidbody;
    private WeaponProperties m_WeaponProperties;

    private List<Vector3> m_DistancePoints;

    private void Awake()
    {
        m_Rigidbody = GetComponent<Rigidbody>();
    }

    public void Initalize(Player ownerIndex, WeaponProperties weaponInfo, Vector3 aimDirection)
    {
        m_Owner = ownerIndex;
        m_WeaponProperties = weaponInfo;

        m_Rigidbody.mass = weaponInfo.Mass;
        m_Rigidbody.AddForce(aimDirection * weaponInfo.InitialForce);

        AddDistancePoint();
        StartCoroutine(TrackDistance());
        StartCoroutine(AutoDestroy());
    }

	private void OnCollisionEnter(Collision collision)
	{
        Explode(collision.transform.GetComponent<Player>());
    }

    private void Explode(Player p)
    {
        if (m_HasExploded)
            return;

        // Create explosion
        Explosion e = Instantiate(m_WeaponProperties.Explosion, transform.position, Quaternion.Euler(0, 0, 0));
        e.Initalize(m_Owner, m_WeaponProperties.ExplosionProperties, GenerateHitProperties(p));
        
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

    private IEnumerator TrackDistance()
    {
        while(!m_HasExploded)
        {
            yield return new WaitForSeconds(0.25f);

            AddDistancePoint();
        }
    }

    private IEnumerator AutoDestroy()
    {
        yield return new WaitForSeconds(3.5f);
        Explode(null);
    }

    private void AddDistancePoint()
    {
        if (m_DistancePoints == null)
            m_DistancePoints = new List<Vector3>();

        m_DistancePoints.Add(transform.position);
    }

    private HitProperties GenerateHitProperties(Player collider)
    {
        AddDistancePoint();

        HitProperties h = new HitProperties();

        // Calcualate distance traveled
        float distance = 0;

        for (int i = 1; i < m_DistancePoints.Count; i++)
        {
            distance += Vector3.Distance(m_DistancePoints[i - 1], m_DistancePoints[i]);
        }

        h.TravelDistance = distance;
        Debug.Log("travel distance: " + distance);

        h.AttackerPlayerIndex = m_Owner.Index;
        if (collider != null)
            h.HitPlayerIndex = collider.Index;
        else
            h.HitPlayerIndex = -1;

        return h;
    }
}
