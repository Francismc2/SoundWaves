using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Collide : MonoBehaviour
{

    private void OnCollisionEnter(Collision collision)
    {
        // Collider Hit
        Collider hitCollider = collision.collider;

        // Contact Point (first contact)
        Vector3 contactPoint = collision.contacts[0].point;

        Debug.Log("Collider Hit: " + hitCollider.name);
        Debug.Log("Contact Point: " + contactPoint);
    }
}

