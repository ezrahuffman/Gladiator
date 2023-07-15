using System;
using System.Collections.Generic;
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

    public delegate void OnTakeDmg(float trueDmg, GameObject dmgSource, Vector3 forceDir, Vector3 hitPos);
    public OnTakeDmg onTakeDmg;

    public delegate void OnReducedToNoHealth(HealthSystem healthSystem);
    public OnReducedToNoHealth onReducedToNoHealth;
    
    public delegate void OnHealthChanged(float maxHealth, float currHealth);
    public OnHealthChanged onHealthChanged;

    [Tooltip("The time in seconds before the same attack can deal dmg. Meant to stop unintended multiple hits from single attacks.")]
    [SerializeField] private float _attackTimeOut = 0.5f;

    protected HashSet<GameObject> _attackers = new HashSet<GameObject>();

    private void Start()
    {
        _health = maxHealth;
    }


    public void RecieveDmg(float rawDmg, GameObject dmgSource, Vector3 forceDir, Vector3 hitPos)
    {
        // Don't take dmg if already dead
        if(_health <= 0)
        {
            return;
        }

       // Don't take dmg from same source twice in a row
       if(_attackers.Contains(dmgSource))
        {
            return;
        }
       _attackers.Add(dmgSource);

        float trueDmg = rawDmg / armor;

        SetHealth(_health - trueDmg);

        onTakeDmg?.Invoke(trueDmg, dmgSource, forceDir, hitPos);
       

        Invoke(nameof(RemoveAttacker), _attackTimeOut);
        
    }

    private void RemoveAttacker()
    {
        _attackers.Clear();
    }

    private void SetHealth(float value) 
    {
        float ogHealth = _health;

        value = Math.Clamp(value, 0, maxHealth);

        _health = value;

        if (_health != ogHealth && onHealthChanged != null)
        {
            onHealthChanged.Invoke(maxHealth, _health);
        }

        if (_health == 0 && onReducedToNoHealth != null)
        {
            onReducedToNoHealth.Invoke(this);
        }
        
    }

    public float Health
    {
        get
        {
            return _health;
        }
    }
}
