using UnityEngine;
using UnityEngine.Assertions;
using System.Collections;

public class Notamari : MonoBehaviour
{
    public Transform m_CameraAnchor;

    public float m_MovementTorque = 10f;

    Rigidbody m_RigidBody;

    void Start()
    {
        Assert.IsNotNull(m_CameraAnchor);

        m_RigidBody = GetComponent<Rigidbody>();
        Assert.IsNotNull(m_RigidBody);
    }

    void OnCollisionEnter(Collision collision)
    {
        Debris debris = collision.gameObject.GetComponent<Debris>();
        if (debris)
        {
            Debug.Log("Collided with debris!");

            // Wipe the debris' rigid body so it becomes part of us
            Destroy(collision.gameObject.GetComponent<Rigidbody>());

            // Make it a child of us
            collision.gameObject.transform.SetParent(transform);

            // Now it's hard to move around!

            // Yay!

            // YAY.
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
