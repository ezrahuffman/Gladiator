using UnityEngine;
using UnityEngine.SceneManagement;
using StarterAssets;
public class GameOverMenu : MonoBehaviour
{
    // Restart the current scene
    public void OnRestartPressed()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    // Return to main menu
    public void OnMainMenuPressed()
    {
        SceneManager.LoadScene(0); // assumes main menu is build index 0
    }
}
