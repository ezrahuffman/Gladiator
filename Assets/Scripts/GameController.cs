using StarterAssets;
using System.Collections;
using UnityEngine;

public class GameController : MonoBehaviour
{
    int _enemiesRemaining = 0;
    // Start is called before the first frame update
    [SerializeField] private GameObject gameOverScreen;
    [SerializeField] private ThirdPersonController playerController;


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

    void OnEnemyReducedToNoHealth(HealthSystem healthSystem)
    {
        healthSystem.onReducedToNoHealth -= OnEnemyReducedToNoHealth;
        _enemiesRemaining--;

        if(_enemiesRemaining <= 0)
        {
            Debug.Log("GAME OVER ALL ENEMIES ARE DEAD");
            StartCoroutine(StartCountDown(5));
        }
    }

    IEnumerator StartCountDown(int time = 5)
    {
        Debug.Log("Starting countdown");
        for (; time > 0; time -= 1)
        {
            // TODO: Update UI to show the time remaining
            Debug.Log($"Time remaining: {time}");
            yield return new WaitForSeconds(1);
        }
        GameOver();
    }

    void GameOver()
    {
        Debug.Log("GAME OVER ALL ENEMIES ARE DEAD");
        PauseGame();
        gameOverScreen.SetActive(true);
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.Confined;
        playerController.LockCameraPosition = true;
    }

    void PauseGame()
    {
        //Time.timeScale = 0; // TODO: Implement a better pause/unpause
    }

    void UnPauseGame()
    {
        Time.timeScale = 1; 
    }

    void OnPlayerReducedToNoHealth(HealthSystem healthSystem)
    {
        Debug.Log("GAME OVER PLAYER HAS DIED");
    }
}
