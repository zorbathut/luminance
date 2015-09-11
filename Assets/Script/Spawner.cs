using UnityEngine;
using UnityEngine.Assertions;
using System.Collections;

public class Spawner : MonoBehaviour
{
    public int m_Spawns;
    public float m_CenterHole;

    public Transform m_Debris;

    public void Spawn()
    {
        Collider spawnBounds = GetComponent<Collider>();
        Assert.IsNotNull(spawnBounds);

        Collider debrisCollider = m_Debris.GetComponent<Collider>();
        Assert.IsNotNull(debrisCollider);

        // this is hacky and will cause things to levitate; fix it later
        float groundOffset = (debrisCollider.bounds.max - debrisCollider.bounds.min).magnitude;

        if (spawnBounds && debrisCollider)
        {
            int spawned = 0;
            int emergencyEscape = 0;    // just to be sure we don't infinite-loop ourselves - this will be slow but it won't crash, and it's on startup anyway
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
                RaycastHit hitInfo;
                if (!Physics.Raycast(position, Vector3.down * 10, out hitInfo))
                {
                    continue;
                }

                // Place it there
                Debris debris = ((Transform)Instantiate(m_Debris, hitInfo.point + Vector3.up * groundOffset, Random.rotationUniform)).GetComponent<Debris>();
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
