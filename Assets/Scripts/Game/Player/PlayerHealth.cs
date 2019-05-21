using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    private float m_Health;
    private float m_MaxHealth;

    public event Action Death;

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
        // Do special things only when damaged
        AddHealth(-Mathf.Clamp(damageAmount, 0, Mathf.Infinity));
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
        if (Death != null)
            Death.Invoke();

        // Since we destroy this instance we remove all subscribers
        Death = null;
    }
}
