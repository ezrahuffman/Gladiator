using UnityEngine;

public class EnemyController : MonoBehaviour
{
    HealthSystem healthSystem;

    [SerializeField] private HealthBar healthBar;

    private Animator _animator;
    private bool _hasAnimator;
    private int _animIDHitTrigger;

    void Start()
    {
        healthSystem = GetComponent<HealthSystem>();
        if(healthSystem != null)
        {
            healthSystem.onTakeDmg += OnTakeDamage;
            healthSystem.onReducedToNoHealth += OnReducedToNoHealth;
            healthSystem.onHealthChanged += OnHealthChanged;
        }

        _hasAnimator = TryGetComponent(out _animator);
        if (_hasAnimator)
        {
            _animIDHitTrigger = Animator.StringToHash("HitTrigger");
        }
    }

    protected virtual void OnTakeDamage(float dmg, GameObject dmgSource)
    {
        Debug.Log($"Took ${dmg} from ${dmgSource}");
        if(_hasAnimator)
        {
            _animator.SetTrigger(_animIDHitTrigger);
        }
    }

    protected virtual void OnReducedToNoHealth(HealthSystem healthSystem)
    {
        Debug.Log($"{gameObject} has been reduced to no health");
    }

    protected virtual void OnHealthChanged(float maxHealth, float currHealth)
    {
        healthBar.UpateHealthbar(maxHealth, currHealth);
    }
}
