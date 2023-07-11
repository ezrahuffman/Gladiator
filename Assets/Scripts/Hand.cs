using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hand : MonoBehaviour
{
    public delegate void HandCollision(Collision collision);
    public HandCollision OnHandCollision;

    

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log($"Hand collided with {collision.gameObject.name}");
        OnHandCollision?.Invoke(collision);
    }
}
