using UnityEngine;
using Photon.Pun;
using UnityEngine.SceneManagement;
using System.IO;

public class RoomManager : MonoBehaviourPunCallbacks
{
    public static RoomManager Instance;
    void Awake()
    {
        if (Instance)
        {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);
        Instance = this;    
    }

    public override void OnEnable()
    {
        base.OnEnable();
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    public override void OnDisable()
    {
        base.OnDisable();
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode loadScenemMode)
    {
        if (scene.buildIndex == 1)
        {
            PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "PlayerManager"), Vector3.zero, Quaternion.identity);
        }
    }

    public void CleanUp()
    {
        if (PhotonNetwork.IsMasterClient) PhotonNetwork.DestroyAll(); // Destroy all Photon network instantiated objects
    }

    // Handle leaving the room or returning to the main menu
    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom(); // Leave the current room
        CleanUp(); // Clean up instantiated objects
    }

    public void ReturnToMenu()
    {
        LeaveRoom(); // Leave the current room
        SceneManager.LoadScene(0); // Load the main menu scene
        Destroy(gameObject); // Destroy the RoomManager instance
    }

}
