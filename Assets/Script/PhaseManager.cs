using UnityEngine;
using System.Collections.Generic;

public class PhaseManager : MonoBehaviour
{
    public List<Spawner> m_Spawners;

    public bool m_First;
    public PhaseManager m_Chain;

    int m_DebrisTotal = 0;

    void Start()
    {
        foreach (Spawner spawner in m_Spawners)
        {
            m_DebrisTotal += spawner.m_Spawns;
        }

        if (m_First)
        {
            BeginPhase();
        }
    }

    void BeginPhase()
    {
        foreach (Spawner spawner in m_Spawners)
        {
            spawner.Spawn();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        Notamari notamari = other.GetComponent<Notamari>();
        if (notamari)
        {
            notamari.SetPhaseManager(this);
        }
    }

    public void NotifyGrabbed()
    {
        // TODO: useful things
    }
}
