using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hand : MonoBehaviour
{
    public delegate void HandCollision(Collision collision, Vector3 handPos);
    public HandCollision OnHandCollision;

    

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            return;
        }

        OnHandCollision?.Invoke(collision, transform.position);
    }
}
