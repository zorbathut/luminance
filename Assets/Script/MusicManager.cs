using UnityEngine;
using System.Collections;

// Simple persistent singleton, nothin' fancy
public class MusicManager : MonoBehaviour {
    static MusicManager m_Instance = null;

	void Start()
    {
        if (m_Instance)
        {
            Destroy(this.gameObject);
        }
        else
        {
            m_Instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
	}
}
