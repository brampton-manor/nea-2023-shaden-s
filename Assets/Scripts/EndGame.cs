using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Photon.Pun;
using UnityEngine.SceneManagement;
using Photon.Realtime;
using UnityEngine.Networking;


public class EndGame : MonoBehaviour
{
    [SerializeField] TMP_Text GameOverText;
    [SerializeField] TMP_Text MatchStatsText;

    Player player;

    int kill;
    int downs;

    string currentWave;

    void Start()
    {
        player = PhotonNetwork.LocalPlayer;
        currentWave = EnemySpawner.Instance.GetWave();
        if (player.CustomProperties.TryGetValue("kills", out object kills))
        {
            kill = (int)kills;
        }
        if (player.CustomProperties.TryGetValue("deaths", out object deaths))
        {
            downs = (int)deaths;
        }
        MatchStatsText.text = "Match Stats\nWaves Survived: " + currentWave + "\nEnemies Killed: " + kill + "\nTimes Downed: " + downs;
        StartCoroutine(EndGameUpdate());
    }

    public void ReturnToMenu()
    {
        RoomManager.Instance.ReturnToMenu();
    }

    public IEnumerator EndGameUpdate()
    {
        WWWForm form = new WWWForm();
        form.AddField("username", player.NickName);
        form.AddField("kills", kill);
        form.AddField("downed", downs);
        form.AddField("wave", currentWave);

        using (UnityWebRequest www = UnityWebRequest.Post("http://localhost/NEA/EndGame.php", form))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError(www.error);
            }
            else
            {
                Debug.Log("Data sent successfully");
                Debug.Log((www.downloadHandler.text));
            }    
        }
    }

}
