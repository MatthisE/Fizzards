using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleScreen : MonoBehaviour
{
    public void OnStartClicked()
    {
        SceneManager.LoadScene("Lobby");
    }

    public void OnQuitClicked()
    {
        // Quit the application
        Application.Quit();
    }
}
