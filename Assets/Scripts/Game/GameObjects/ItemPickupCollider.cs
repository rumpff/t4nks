using System;
using UnityEngine;

public class ItemPickupCollider : MonoBehaviour
{
    public Action<Player> PickupEvent;

    private void OnTriggerEnter(Collider other)
    {
        Player p = other.GetComponentInParent<Player>();

        if (p != null)
            PickupEvent(p);
    }
}
