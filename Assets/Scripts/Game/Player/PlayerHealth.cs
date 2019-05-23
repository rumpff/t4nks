using System;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    private float m_Health;
    private float m_MaxHealth;

    public delegate void Damage(float damage);

    public event Damage DamageEvent;
    public event Action DeathEvent;

    public void InitalizeHealth(float maxHealth)
    {
        m_Health = maxHealth;
        m_MaxHealth = maxHealth;
    }

    public void SetHealth(float newHealth)
    {
        m_Health = newHealth;
        CheckHealth();
    }

    public void DamagePlayer(float damageAmount)
    {
        if (damageAmount <= 0)
            return;

        // Do special things only when damaged
        AddHealth(-damageAmount);

        if (DamageEvent != null)
            DamageEvent.Invoke(damageAmount);
    }

    public void AddHealth(float addAmount)
    {
        m_Health += addAmount;
        CheckHealth();
    }


    private void CheckHealth()
    {
        // Check if the player is dead
        if (m_Health <= 0.0f)
            OnDeath();
    }

    protected virtual void OnDeath()
    {
        if (DeathEvent != null)
            DeathEvent.Invoke();

        // Since we destroy this instance we remove all subscribers
        DeathEvent = null;
    }

    public float Health
    {
        get { return m_Health; }
    }

    public float MaxHealth
    {
        get { return m_MaxHealth; }
    }
}
