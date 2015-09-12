using UnityEngine;
using UnityEngine.Assertions;
using System.Collections.Generic;

// Not a Katamari. Get it? Notamari? Yeah not the most inspired naming ever.
// Player class, handles all player-entity-specific game logic.
public class Notamari : MonoBehaviour
{
    public Transform m_CameraAnchor;
    public Transform m_CentralLightsource;

    // Movement tuning
    public float m_MovementTorqueBase = 10f;
    public float m_MovementTorquePerDebris = 0.5f;

    // Collision addition tuning
    public float m_SphereAbsorption = 2f;
    public float m_SphereExpansion = 2f;

    // Visuals tuning
    public float m_CollectedPctPower = 0.5f;
    public float m_VisualsLerp = 0.05f;

    // Audio
    public List<AudioClip> m_Sounds;
    int m_SoundLast = -1;
    int m_SoundLastPrev = -1;

    Rigidbody m_RigidBody;
    Renderer m_Renderer;

    // Stored from the current active phase; this is kind of ugly
    int m_DebrisTotal = 1000;   // meaningless number meant to be "very large", just to avoid divide-by-zero on the first SyncDebrisProperties

    // Stored to lerp visuals
    float m_LastCollectedPct = 0;
    Color m_LastDebrisColor = new Color();

    List<Debris> m_Children = new List<Debris>();

    float m_MovementTorque;

    PhaseManager m_CurrentPhase;

    void Start()
    {
        Assert.IsNotNull(m_CameraAnchor);

        m_RigidBody = GetComponent<Rigidbody>();
        Assert.IsNotNull(m_RigidBody);

        m_Renderer = GetComponent<Renderer>();
        Assert.IsNotNull(m_Renderer);

        m_MovementTorque = m_MovementTorqueBase;

        SyncDebrisProperties();
    }

    public void SetPhaseManager(PhaseManager newManager, int debrisTotal)
    {
        m_CurrentPhase = newManager;
        m_DebrisTotal = debrisTotal;
        SyncDebrisProperties();
    }

    public void UnsetPhaseManager(PhaseManager oldManager)
    {
        // Unset only if we're actually leaving the current phase
        if (m_CurrentPhase == oldManager)
        {
            m_CurrentPhase = null;
        }
    }

    public int GetDebrisCount()
    {
        return m_Children.Count;
    }

    // Entire purpose of this is to pick up debris
    void OnCollisionEnter(Collision collision)
    {
        Debris debris = collision.gameObject.GetComponent<Debris>();
        if (debris && debris.transform.parent != transform) // Extra test to make sure we haven't already picked it up and are colliding with another element!
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

            // Pick a new random sound we haven't recently played
            int sound = -1;
            do {
                sound = Random.Range(0, m_Sounds.Count);
            } while (sound == m_SoundLast || sound == m_SoundLastPrev);
            m_SoundLastPrev = m_SoundLast;
            m_SoundLast = sound;

            // Play it!
            AudioSource source = debris.GetComponent<AudioSource>();
            source.clip = m_Sounds[sound];
            source.Play();

            // Update the phase if necessary (it should be necessary!)
            if (m_CurrentPhase)
            {
                m_CurrentPhase.NotifyGrabbed(this);
            }
        }
    }

    // Processes new colors, lighting, and shader parameters
    void SyncDebrisProperties()
    {
        float collectedPct = m_Children.Count / (float)m_DebrisTotal;
        collectedPct = Mathf.Pow(collectedPct, m_CollectedPctPower);

        m_LastCollectedPct = Mathf.Lerp(m_LastCollectedPct, collectedPct, m_VisualsLerp);

        // Update our visible look
        if (m_Renderer)
        {
            m_Renderer.material.SetFloat("_ClipThreshold", 1 - m_LastCollectedPct);
        }

        // Update our physics; without this, it gets really hard to move
        m_MovementTorque = m_MovementTorqueBase + m_MovementTorquePerDebris * m_Children.Count;

        // Update children intensity and accumulate lighting color
        Color debrisColor = new Color();
        foreach (Debris debris in m_Children)
        {
            debris.SetIntensity(1 - m_LastCollectedPct);
            debrisColor += debris.GetBaseColor();
        }
        if (m_Children.Count > 0)
        {
            debrisColor /= m_Children.Count;
        }
        else
        {
            debrisColor = new Color();
        }
        debrisColor *= m_LastCollectedPct;
        m_LastDebrisColor = Color.Lerp(m_LastDebrisColor, debrisColor, m_VisualsLerp);

        // Update central lightsource
        Assert.IsNotNull(m_CentralLightsource);
        if (m_CentralLightsource)
        {
            Light light = m_CentralLightsource.GetComponent<Light>();
            Renderer renderer = m_CentralLightsource.GetComponent<Renderer>();

            Assert.IsNotNull(light);
            if (light)
            {
                light.color = m_LastDebrisColor;
            }

            Assert.IsNotNull(renderer);
            if (renderer)
            {
                renderer.material.SetColor("_TintColor", m_LastDebrisColor);
            }
        }

        // Finally, if we've collected everything, get rid of all our child colliders so the sphere runs smoothly (for like the next three seconds but it was distracting before I did this)
        if (m_Children.Count == m_DebrisTotal)
        {
            foreach (Debris debris in m_Children)
            {
                Destroy(debris.GetComponent<Collider>());
            }
        }
    }

    public void Empty()
    {
        foreach (Debris debris in m_Children)
        {
            Destroy(debris.gameObject);
        }
        m_Children.Clear();
    }

    void FixedUpdate()
    {
        // This is what moves the sphere
        m_RigidBody.AddTorque(new Vector3(Input.GetAxis("Vertical"), 0, -Input.GetAxis("Horizontal")) * m_MovementTorque);

        // Technically this should be in Update(), but then the math to make the lerp work properly is a lot more difficult, and nobody will notice it *only* updating at 60fps
        SyncDebrisProperties();
    }

    void Update()
    {
        if (m_CameraAnchor)
        {
            // Move the camera along with the sphere; it's not a child of the sphere so we don't have to muck about with undoing rotations
            float targetY = transform.position.y;

            // If we're in a phase, we lock the camera to the phase's vertical position so that slopes and bumps look correct
            if (m_CurrentPhase)
            {
                targetY = m_CurrentPhase.transform.position.y;
            }

            // this is not the right delta-time behavior, but it's close enough for now
            m_CameraAnchor.transform.position = new Vector3(transform.position.x, Mathf.Lerp(m_CameraAnchor.transform.position.y, targetY, 1 * Time.deltaTime), transform.position.z);
        }
    }
}
