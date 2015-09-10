using UnityEngine;
using UnityEngine.Assertions;
using System.Collections.Generic;

public class Notamari : MonoBehaviour
{
    public Transform m_CameraAnchor;

    public float m_MovementTorqueBase = 10f;
    public float m_MovementTorquePerDebris = 0.5f;
    public float m_SphereAbsorption = 2f;
    public float m_SphereExpansion = 2f;

    float m_MovementTorque;

    Rigidbody m_RigidBody;
    Renderer m_Renderer;

    int m_DebrisTotal = 0;

    List<Debris> m_Children = new List<Debris>();

    void Start()
    {
        Assert.IsNotNull(m_CameraAnchor);

        m_RigidBody = GetComponent<Rigidbody>();
        Assert.IsNotNull(m_RigidBody);

        m_Renderer = GetComponent<Renderer>();
        Assert.IsNotNull(m_Renderer);

        // this is not an elegant solution; it is a one-line solution
        m_DebrisTotal = FindObjectsOfType<Debris>().Length;

        m_MovementTorque = m_MovementTorqueBase;
    }

    void OnCollisionEnter(Collision collision)
    {
        Debris debris = collision.gameObject.GetComponent<Debris>();
        if (debris && debris.transform.parent != transform)
        {
            m_Children.Add(debris);

            // Wipe the debris' rigid body so it becomes part of us
            Destroy(debris.GetComponent<Rigidbody>());

            // Make it a child of us
            debris.transform.SetParent(transform);

            // Now it's hard to move around! Move the sphere closer to our center, then expand it, in an attempt to flatten things out and make it easier to move around
            SphereCollider debrisCollider = debris.GetComponent<SphereCollider>();
            Assert.IsNotNull(debrisCollider);
            if (debrisCollider)
            {
                // Deal with some annoying frames of references
                // Take the debris's position in ballspace, move towards the center of the ball by (radius * sphereAbsorption)
                // (we pick x out of the scale because it should be uniformly scaled)
                Vector3 intendedCenter = debris.transform.localPosition - debris.transform.localPosition.normalized * debris.transform.localScale.x * m_SphereAbsorption;

                // Now convert that point to worldspace
                Vector3 intendedCenterWorld = transform.TransformPoint(intendedCenter);

                // Now convert that point to debrisspace
                debrisCollider.center = debris.transform.InverseTransformPoint(intendedCenterWorld);

                // Finally, expand the sphere as planned
                debrisCollider.radius *= m_SphereExpansion;

                // There is probably a faster way to do this but whatever there's like fifty pieces of debris per level at most
            }

            SyncDebrisProperties();
        }
    }

    // Called whenever children count changes
    void SyncDebrisProperties()
    {
        float collectedPct = m_Children.Count / (float)m_DebrisTotal;

        // Update our visible look
        if (m_Renderer)
        {
            m_Renderer.material.SetFloat("_ClipThreshold", 1 - collectedPct);
        }

        // Update our physics; without this, it gets really hard to move
        m_MovementTorque = m_MovementTorqueBase + m_MovementTorquePerDebris * m_Children.Count;

        // Update children
        foreach (Debris debris in m_Children)
        {
            debris.SetIntensity(1 - collectedPct);
        }
    }

    void FixedUpdate()
    {
        m_RigidBody.AddTorque(new Vector3(Input.GetAxis("Vertical"), 0, -Input.GetAxis("Horizontal")) * m_MovementTorque);
    }

    void Update()
    {
        if (m_CameraAnchor)
        {
            // Move the camera along with the sphere; it's not a child of the sphere so we don't have to muck about with undoing rotations
            m_CameraAnchor.transform.position = new Vector3(transform.position.x, 0f, transform.position.z);
        }
        
    }
}
