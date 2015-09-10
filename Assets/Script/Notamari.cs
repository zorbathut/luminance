using UnityEngine;
using UnityEngine.Assertions;
using System.Collections;

public class Notamari : MonoBehaviour
{
    public Transform m_CameraAnchor;

    public float m_MovementForce = 10f;

    Rigidbody m_RigidBody;

    void Start()
    {
        Assert.IsNotNull(m_CameraAnchor);

        m_RigidBody = GetComponent<Rigidbody>();
        Assert.IsNotNull(m_RigidBody);
    }
        
    void FixedUpdate()
    {
        m_RigidBody.AddForce(new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical")) * m_MovementForce);
    }

    void Update()
    {
        if (m_CameraAnchor)
        {
            // Move the camera along with the sphere; it's not a child of the sphere so we don't have to muck about with undoing rotations
            m_CameraAnchor.transform.position = transform.position;
        }
        
    }
}
