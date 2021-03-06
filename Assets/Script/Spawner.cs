﻿using UnityEngine;
using UnityEngine.Assertions;
using System.Collections;

// Debris spawner. Places debris on surfaces so it doesn't roll around awkwardly.
public class Spawner : MonoBehaviour
{
    public int m_Spawns;
    public float m_CenterHole;  // prevents debris from spawning literally on the player; want to make sure picking up the first one is a conscious movement

    public Transform m_Debris;

    public void Spawn()
    {
        Collider spawnBounds = GetComponent<Collider>();
        Assert.IsNotNull(spawnBounds);

        Collider debrisCollider = m_Debris.GetComponent<Collider>();
        Assert.IsNotNull(debrisCollider);

        // this is a hacky way of figuring out how high off the ground things should be. seems to work though
        float groundOffset = (debrisCollider.bounds.max - debrisCollider.bounds.min).magnitude;

        if (spawnBounds && debrisCollider)
        {
            // The original plan for this loop was that I'd be able to put it around pits and stuff and it would still place the right number of debris elements
            // Then I never ended up making any pits
            // Whoops
            // So I guess this entire bit of complexity is useless. Should probably take it out just for bug prevention, but I'm leaving it for posterity :V
            int spawned = 0;
            int emergencyEscape = 0;    // just to be sure we don't infinite-loop ourselves - this will be slow but it won't crash, and it's on startup/during a phase change anyway
            int emergencyEscapeThreshold = m_Spawns * 100;
            while (spawned < m_Spawns && emergencyEscape++ < emergencyEscapeThreshold)
            {
                // Choose random place
                Vector3 position = new Vector3(
                    Random.Range(spawnBounds.bounds.min.x, spawnBounds.bounds.max.x),
                    spawnBounds.bounds.max.y,
                    Random.Range(spawnBounds.bounds.min.z, spawnBounds.bounds.max.z)
                );

                if (position.x * position.x + position.z * position.z < m_CenterHole)
                {
                    continue;
                }

                // Trace down so we find where it should land
                RaycastHit[] hits = Physics.RaycastAll(position, Vector3.down);
                Vector3 hitPoint = new Vector3();
                float hitPointDistance = float.PositiveInfinity;

                foreach (RaycastHit hit in hits)
                {
                    if (hit.collider.GetComponent<Renderer>() && hitPointDistance > hit.distance)
                    {
                        // Find the closest visible collider
                        hitPointDistance = hit.distance;
                        hitPoint = hit.point;
                    }
                }

                if (hitPointDistance == float.PositiveInfinity)
                {
                    // Did not find any impact point; don't include this object!
                    continue;
                }

                // Place it there
                Debris debris = ((Transform)Instantiate(m_Debris, hitPoint + Vector3.up * groundOffset, Random.rotationUniform)).GetComponent<Debris>();
                ++spawned;

                // Set up colors
                Assert.IsNotNull(debris);
                if (!debris)    // test after we increment spawned, just so we don't end up creating literally thousands of objects if this fails
                {
                    continue;
                }

                debris.SetColor(Random.Range(0f, 1f));
            }

            Assert.IsTrue(emergencyEscape < emergencyEscapeThreshold);
        }
    }
}
