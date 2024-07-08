using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class Leaderboard : MonoBehaviour
{
    public static Leaderboard Instance;
    public TMP_Text LeaderboardText;
    public Button LeaderboardButton;

    public IEnumerator ShowLeaderboard()
    {
        using (UnityWebRequest www = UnityWebRequest.Get("http://localhost/NEA/Leaderboard.php"))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                LeaderboardText.text = www.error;
            }
            else
            {
                LeaderboardText.text = www.downloadHandler.text;
            }
        }
    }

    public void UpdateLeaderboard()
    {
        StartCoroutine(ShowLeaderboard());
    }

    void Start()
    {
        Instance = this;
        LeaderboardButton.onClick.AddListener(() =>
        {
            StartCoroutine(ShowLeaderboard());
        });
    }
}
