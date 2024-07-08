using UnityEngine;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] PlayerController player;
    [SerializeField] public Canvas SettingsMenu;
    public void GoBack()
    {
        player.Resume();
    }

    public void OpenSettings()
    {
        player.PauseMenu.gameObject.SetActive(false);
        SettingsMenu.gameObject.SetActive(true);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
