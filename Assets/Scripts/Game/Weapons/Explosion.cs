using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Explosion : MonoBehaviour
{
    private ExplosionProperties m_Properties;
    private bool m_IsInitialized = false;

    private float m_LifeTime = 0;

    public void Initalize(ExplosionProperties properties)
    {
        m_Properties = properties;

        DamageNearbyPlayers();

        m_IsInitialized = true;
    }

    private void DamageNearbyPlayers()
    {
        Collider[] explosionCollisions = Physics.OverlapSphere(transform.position, m_Properties.ExplosionRadius);
        List<Player> collidedPlayers = new List<Player>();
        
        // Obtain all the players within the explosion radius
        for (int i = 0; i < explosionCollisions.Length; i++)
        {
            Player p = explosionCollisions[i].transform.root.GetComponent<Player>();

            // Check if the root has a player component
            if (p == null)
                continue;

            // Check if we've already added this player to the list
            if (collidedPlayers.Contains(p))
                continue;

            // Add the player to the list
            collidedPlayers.Add(p);
        }

        // Check if we've collided with any player
        if (collidedPlayers.Count == 0)
            return;

        // Calculate and apply all the effects to the players
        for (int i = 0; i < collidedPlayers.Count; i++)
        {
            Player p = collidedPlayers[i];

            // Subtract health
            float playerDistance = Vector3.Distance(transform.position, p.transform.position);
            float damageDropoff = (playerDistance / m_Properties.ExplosionRadius);
            float damage = m_Properties.BaseDamage - (m_Properties.BaseDamage * damageDropoff);

            Debug.Log("Damage: " + damage);

            p.Health.DamagePlayer(damage);

            // Add explosion force
            p.Rigidbody.AddExplosionForce(m_Properties.ExplosionForce, transform.position, m_Properties.ExplosionRadius);
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
