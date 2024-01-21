using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Networking;
using Photon.Pun;

public class Login : MonoBehaviour
{
    public static Login Instance;

    public TMP_InputField UsernameInput;
    public TMP_InputField PasswordInput;
    public TMP_Text DebugText;
    public Button LoginButton;

    public bool LoggedIn = false;

    public IEnumerator LoginAccount(string username, string password)
    {
        WWWForm form = new WWWForm();
        form.AddField("loginUser", username);
        form.AddField("loginPass", password);

        using (UnityWebRequest www = UnityWebRequest.Post("http://localhost/NEA/Login.php", form))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                DebugText.text = www.error;
            }
            else
            {
                DebugText.text = www.downloadHandler.text;
                PhotonNetwork.NickName = username;
                LoggedIn = true;
            }
        }
    }
    void Start()
    {
        Instance = this;
        LoginButton.onClick.AddListener(() =>
        {
            StartCoroutine(LoginAccount(UsernameInput.text, PasswordInput.text));
        });
    }

}
