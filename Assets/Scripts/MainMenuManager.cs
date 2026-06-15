using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    public void PlayGame()
    {
        SceneManager.LoadScene("SampleScene");
    }

    public void OpenSettings()
    {
        SettingsMenuManager.Instance?.Open();
    }

    public void ExitGame()
    {
        Debug.Log("EXIT PRESSED");
        Application.Quit();
    }
}