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
}
