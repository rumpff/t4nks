using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthBlock : MonoBehaviour
{
    [SerializeField]
    private HealthPickup m_HealthPickup;

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("trigga!");
        Player p = other.GetComponentInParent<Player>();

        if (p != null)
            m_HealthPickup.HealthPickedUp(p);
    }
}
