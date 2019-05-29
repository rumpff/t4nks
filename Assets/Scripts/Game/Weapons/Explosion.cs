using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Explosion : MonoBehaviour
{
    private Player m_Owner;
    private ExplosionProperties m_Properties;
    private HitProperties m_HitProperties;
    private bool m_IsInitialized = false;

    private float m_LifeTime = 0;

    public void Initalize(Player owner, ExplosionProperties eProperties, HitProperties hProperties)
    {
        if (eProperties == null)
        {
            Debug.LogError("Null reference explosion properties");
            Destroy(gameObject);
        }           

        m_Owner = owner;
        m_Properties = eProperties;
        m_HitProperties = hProperties;

        ExplosionBehaviour();

        m_IsInitialized = true;
    }

    private void ExplosionBehaviour()
    {
        Collider[] explosionCollisions = Physics.OverlapSphere(transform.position, m_Properties.ExplosionRadius);
        List<Player> collidedPlayers = new List<Player>();
        List<Explodable> collidedExplodables = new List<Explodable>();

        // Obtain all the explodables within the explosion radius
        for (int i = 0; i < explosionCollisions.Length; i++)
        {
            // Check for players
            Player p = explosionCollisions[i].transform.root.GetComponent<Player>();

            // Check if the root has a player component
            // and check if we've already added this player to the list
            if (p != null && !collidedPlayers.Contains(p))
                collidedPlayers.Add(p);

            // Check for explodables
            Explodable e = explosionCollisions[i].transform.root.GetComponent<Explodable>();

            if (e != null && !collidedExplodables.Contains(e))
                collidedExplodables.Add(e);
        }

        // Calculate and apply all the effects to the players
        for (int i = 0; i < collidedPlayers.Count; i++)
        {
            Player p = collidedPlayers[i];

            // Subtract health
            float playerDistance = Vector3.Distance(transform.position, p.transform.position);
            float damageDropoff = ((float)playerDistance / (float)m_Properties.ExplosionRadius);
            float damage = m_Properties.BaseDamage - (m_Properties.BaseDamage * damageDropoff);

            // Lower the damage if it is self-inflicted
            if(damage > 0)
            {
                if (p == m_Owner)
                {
                    damage /= 3;
                    GameManager.I.AddToStat(m_Owner.Index, StatTypes.SelfDamage, damage);
                }
                else
                {
                    // Add damage to stats
                    GameManager.I.AddToStat(m_Owner.Index, StatTypes.DamageDealt, damage);
                    GameManager.I.AddToStat(p.Index, StatTypes.DamageRecieved, damage);
                    GameManager.I.AddScore(m_Owner.Index, damage * GameManager.I.Rules.ScorePerDamageMultiplier, "damage");
                }

                p.Health.DamagePlayer(damage, m_Owner);
            }               

            p.Camera.AddScreenshake(3.0f - (3.0f * damageDropoff));
        }

        for (int i = 0; i < collidedExplodables.Count; i++)
        {
            // Add explosion force
            collidedExplodables[i].Rigidbody.AddExplosionForce(m_Properties.ExplosionForce, transform.position, m_Properties.ExplosionRadius);
        }

        // Stats
        if(collidedPlayers.Count > 0)
        {
            if(m_HitProperties.TravelDistance >= GameManager.I.Rules.LonghitThreshold)
            {
                GameManager.I.AddScore(m_Owner.Index, StatTypes.LongHit);
            }
        }
    }

    void Update()
    {
        if (!m_IsInitialized)
            return;

        m_LifeTime += Time.deltaTime;
        if (m_LifeTime >= m_Properties.MaxLifeTime)
            Destroy(gameObject);
    }

    void OnDrawGizmos()
    {
        if (!m_IsInitialized)
            return;

        Gizmos.color = new Color32(255, 0, 0, 50);
        Gizmos.DrawSphere(transform.position, m_Properties.ExplosionRadius);
    }
}
