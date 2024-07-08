using System.Collections;
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
                if (www.downloadHandler.text == "SUCCESS")
                {
                    PhotonNetwork.NickName = username;
                    DebugText.text = "LOGGED IN";
                    LoggedIn = true;
                    Profile.Instance.UpdateProfile();
                }
                else
                {
                    DebugText.text = www.downloadHandler.text;
                }
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
