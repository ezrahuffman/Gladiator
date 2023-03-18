using System;
using UnityEngine;

[System.Serializable]
public class HealthSystem : MonoBehaviour
{
    
    private float _health;

    [Tooltip("The maximum health the character can have")]
    [SerializeField]
    float maxHealth;

    [SerializeField]
    [Tooltip("Armor mitigates dmg (currently all dmg is mitigated)")]
    public float armor = 1f;

    public delegate void OnTakeDmg(float trueDmg, GameObject dmgSource);
    public OnTakeDmg onTakeDmg;

    public delegate void OnReducedToNoHealth();
    public OnReducedToNoHealth onReducedToNoHealth;

    private void Start()
    {
        _health = maxHealth;
    }


    public void RecieveDmg(float rawDmg, GameObject dmgSource)
    {
        float trueDmg = rawDmg / armor;

        SetHealth(_health - trueDmg);

        if(onTakeDmg != null)
        {
            onTakeDmg.Invoke(trueDmg, dmgSource);
        }
        
    }

    private void SetHealth(float value) 
    {
        //float ogHealth = _health;

        value = Math.Clamp(value, 0, maxHealth);

        _health = value;

        if (_health == 0 && onReducedToNoHealth != null)
        {
            onReducedToNoHealth.Invoke();
        }
        
    }
}
