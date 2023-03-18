using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    HealthSystem healthSystem;

    void Start()
    {
        healthSystem = GetComponent<HealthSystem>();
        if(healthSystem != null)
        {
            healthSystem.onTakeDmg += OnTakeDamage;
            healthSystem.onReducedToNoHealth += OnReducedToNoHealth;
        }
    }

    protected virtual void OnTakeDamage(float dmg, GameObject dmgSource)
    {
        Debug.Log($"Took ${dmg} from ${dmgSource}");
    }

    protected virtual void OnReducedToNoHealth()
    {
        Debug.Log($"{gameObject} has been reduced to no health");
    }
}
