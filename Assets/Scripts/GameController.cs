using UnityEngine;

public class GameController : MonoBehaviour
{
    int _enemiesRemaining = 0;
    // Start is called before the first frame update
    void Start()
    {
        // Get everything with a health system and subscribe to events
        HealthSystem[] healthSystems = GameObject.FindObjectsOfType<HealthSystem>();
        foreach (var healthSystem in healthSystems)
        {
            // Enemy
            if (healthSystem.gameObject.CompareTag("Enemy"))
            {
                healthSystem.onReducedToNoHealth += OnEnemyReducedToNoHealth;
                _enemiesRemaining++;
            }

            // Player
            if (healthSystem.gameObject.CompareTag("Player"))
            {
                healthSystem.onReducedToNoHealth += OnPlayerReducedToNoHealth;
            }
        }
        
    }

    void OnEnemyReducedToNoHealth()
    {
        _enemiesRemaining--;

        if(_enemiesRemaining <= 0)
        {
            Debug.Log("GAME OVER ALL ENEMIES ARE DEAD");
        }
    }

    void OnPlayerReducedToNoHealth()
    {
        Debug.Log("GAME OVER PLAYER HAS DIED");
    }
}
