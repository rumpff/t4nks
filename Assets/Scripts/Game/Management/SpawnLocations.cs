using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnLocations : MonoBehaviour
{
    public readonly float Offset = 5.0f;
    public List<Vector3> Locations;

    void OnDrawGizmosSelected()
    {
        for (int i = 0; i < Locations.Count; i++)
        {
            // Display the explosion radius when selected
            Color c = Color.HSVToRGB((float)i / Locations.Count, 0.6f, 1.0f);
            c.a = 0.5f;

            Gizmos.color = c;
            Gizmos.DrawCube(Locations[i], Vector3.one * Offset);
        }
    }

    public Vector3 RandomLocation()
    {
        System.Random rand = new System.Random(Guid.NewGuid().GetHashCode());
        int index = rand.Next(Locations.Count - 1);

        return Locations[index];
    }

    /// <summary>
    /// Returns the location that's the furthest away from players
    /// </summary>
    /// <returns></returns>
    public Vector3 FurthestLocation(List<Vector3> playerLocations)
    {
        if (playerLocations == null || playerLocations.Count == 0)
            return RandomLocation();

        Vector3 location = Vector3.zero;
        float furthestDistance = 0.0f;

        for (int l = 0; l < Locations.Count; l++)
        {
            float closestDistance = Mathf.Infinity;

            for (int p = 0; p < playerLocations.Count; p++)
            {
                float distance = Vector3.Distance(Locations[l], playerLocations[p]);

                if (distance < closestDistance)
                    closestDistance = distance;
            }

            if(closestDistance > furthestDistance)
            {
                furthestDistance = closestDistance;
                location = Locations[l];
            }
        }

        // Check if nothing has changed
        if (location == Vector3.zero)
            return RandomLocation();

        return location;
    }
}
