using UnityEngine;
using UnityEngine.SceneManagement;

public class StartMenu : MonoBehaviour
{

    // Currently just loads the next scene, maybe not what we want
    // TODO: Laser royal has a basic level select
   public void OnStartGamePressed()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }
}
