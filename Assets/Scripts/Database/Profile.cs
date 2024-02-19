using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class Profile : MonoBehaviour
{
    public static Profile Instance;
    public TMP_Text UsernameText;
    public TMP_Text DebugText;
    public Button ProfileButton;

    public IEnumerator ShowProfile()
    {
        WWWForm form = new WWWForm();
        form.AddField("username", PhotonNetwork.NickName);

        using (UnityWebRequest www = UnityWebRequest.Post("http://localhost/NEA/Profile.php", form))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                DebugText.text = www.error;
            }
            else
            {
                UsernameText.text = PhotonNetwork.NickName;
                DebugText.text = (www.downloadHandler.text);
            }
        }
    }

    public void UpdateProfile()
    {
        StartCoroutine(ShowProfile());
    }

    void Start()
    {
        Instance = this;
        ProfileButton.onClick.AddListener(() =>
        {
            StartCoroutine(ShowProfile());
        });
    }
}
