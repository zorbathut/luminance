using UnityEngine;
using UnityEngine.Assertions;
using System.Collections;

public class Spawner : MonoBehaviour
{
    public int m_Spawns;

    public Transform m_Debris;

    void Start()
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

                // Trace down so we find where it should land
                RaycastHit hitInfo;
                if (!Physics.Raycast(position, Vector3.down, out hitInfo))
                {
                    continue;
                }

                // Place it there
                Instantiate(m_Debris, hitInfo.point + Vector3.up * groundOffset, Random.rotationUniform);
                ++spawned;
            }

            Assert.IsTrue(emergencyEscape < emergencyEscapeThreshold);
        }
    }

}
