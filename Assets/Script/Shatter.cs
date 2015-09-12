using UnityEngine;
using UnityEngine.Assertions;
using System.Collections;

// Destroyed pieces of level. The purpose of this is basically to lerp out after a period of time.
public class Shatter : MonoBehaviour
{
    public float m_LifetimeBase = 3f;
    public float m_LifetimeDeviation = 1f;
    public float m_LifetimeMin = 0.5f;

    float m_LifetimeMaximum;
    float m_LifetimeRemaining;

    Renderer m_Renderer;

	void Start()
    {
        m_LifetimeMaximum = Mathf.Max(m_LifetimeMin, m_LifetimeBase + m_LifetimeDeviation * Util.NextGaussian());
        m_LifetimeRemaining = m_LifetimeMaximum;

        m_Renderer = GetComponent<Renderer>();
        Assert.IsNotNull(m_Renderer);
	}
	
	void Update ()
    {
        m_LifetimeRemaining -= Time.deltaTime;

        if (m_Renderer)
        {
            Color color = m_Renderer.material.color;
            color.a = m_LifetimeRemaining / m_LifetimeMaximum;
            m_Renderer.material.color = color;
        }

        if (m_LifetimeRemaining <= 0)
        {
            Destroy(gameObject);
        }
	}
}
