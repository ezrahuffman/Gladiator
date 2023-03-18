using UnityEngine;

public class EnemyController : MonoBehaviour
{
    HealthSystem healthSystem;

    [SerializeField] private HealthBar healthBar;

    void Start()
    {
        healthSystem = GetComponent<HealthSystem>();
        if(healthSystem != null)
        {
            healthSystem.onTakeDmg += OnTakeDamage;
            healthSystem.onReducedToNoHealth += OnReducedToNoHealth;
            healthSystem.onHealthChanged += OnHealthChanged;
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

    protected virtual void OnHealthChanged(float maxHealth, float currHealth)
    {
        healthBar.UpateHealthbar(maxHealth, currHealth);
    }
}
