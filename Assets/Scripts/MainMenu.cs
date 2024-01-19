using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void LoadLobby()
    {
        SceneManager.LoadScene(0);
    }
    public void LoadLogin()
    {
        SceneManager.LoadScene(4);
    }

    public void LoadTraining()
    {
        SceneManager.LoadScene(3);
    }
}
