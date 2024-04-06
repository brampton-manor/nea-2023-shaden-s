using System.Collections;
using UnityEngine;
using TMPro;
using Photon.Pun;
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

    private bool hasUpdatedEndGame = false;

    public void TriggerEndGameUpdate()
    {
        if (!hasUpdatedEndGame) // Ensure this runs only once
        {
            hasUpdatedEndGame = true; // Prevent multiple executions
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

            MatchStatsText.text = $"Match Stats\nWaves Survived: {currentWave}\nEnemies Killed: {kill}\nTimes Downed: {downs}";
            Debug.Log("End Game Triggered");
            StartCoroutine(EndGameUpdate());
        }
    }

    public void ReturnToMenu()
    {
        RoomManager.Instance.ReturnToMenu();
    }

    public IEnumerator EndGameUpdate()
    {
        WWWForm form = new WWWForm();
        form.AddField("username", PhotonNetwork.NickName);
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
                Debug.Log((www.downloadHandler.text));
            }    
        }
    }

}
