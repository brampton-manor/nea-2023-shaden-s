using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using System.Linq;
using System;
using UnityEngine.TextCore.Text;

public class RegisterUser : MonoBehaviour
{
    public TMP_InputField UsernameInput;
    public TMP_InputField PasswordInput;
    public TMP_Text DebugText;
    public Button RegisterButton;
    
    bool Valid = false;
    public IEnumerator CreateUser(string username, string password)
    {
        WWWForm form = new WWWForm();
        form.AddField("newUser", username);
        form.AddField("newPass", password);

        using (UnityWebRequest www = UnityWebRequest.Post("http://localhost/NEA/RegisterUser.php", form))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                DebugText.text = (www.error);
            }
            else
            {
                DebugText.text = (www.downloadHandler.text);
            }
        }

    }


    bool CheckIfLowerCase(char Character)
    {
        if (char.IsLower(Character)) return true;
        else 
        { 
        DebugText.text = ("Password must contain at least 1 lower case character\n");
        return false;
        }
    }
    bool CheckIfUpperCase(char Character)
    {
        if (char.IsUpper(Character)) return true;
        else
        {
            DebugText.text = ("Password must contain at least 1 upper case character");
            return false;
        }
    }
    bool CheckIfDigit(char Character)
    {
        if (char.IsDigit(Character)) return true;
        else
        {
            DebugText.text = ("Password must contain at least 1 digit");
            return false;
        }
        
    }
    bool CheckIfSymbol(char Character)
    {
        if (char.IsLetterOrDigit(Character))return true;
        else
        {
            DebugText.text = ("Password must contain at  least 1 special character");
            return false;
        }
    }
    bool CheckPasswordLength(string Password)
    {
        if (Password.Length > 7 && Password.Length < 41) return true;
        else
        {
            DebugText.text = ("Password must be between 8 - 40  Characters");
            return false;
        }
    }
    bool PasswordIsValid(string Password)
    {
        Valid = true;
        if (!Password.Any(Character => CheckIfLowerCase(Character))) //Have to do it seperately to get accurate error message
    {
            Valid = false;
        }
        if (!Password.Any(Character => CheckIfUpperCase(Character)))
        {
            Valid = false;
        }
        if (!Password.Any(Character => CheckIfDigit(Character)))
        {
            Valid = false;
        }
        if (!Password.Any(Character => CheckIfSymbol(Character)))
        {
            Valid = false;
        }
        if (!CheckPasswordLength(Password))
        {
            Valid = false;
        }
        return Valid;
    }

    bool CheckUsernameLength(string username)
    {
        if (username.Length > 4 && username.Length < 20) return true;
        else
        {
            DebugText.text = ("Username must be between 4 - 20  Characters");
            return false;
        }
    }

    bool UsernameIsValid(string Username)
    {
        Valid = true;
        if (Username.Any(Character => !char.IsLetterOrDigit(Character)))
        {
            DebugText.text = ("Username must be alphanumeric");
            Valid = false;
        }
        if (!CheckUsernameLength(Username))
        {
            Valid = false;
        }
        return Valid;
    }



    public bool CheckInputs()
    {
        if (PasswordIsValid(PasswordInput.text) && UsernameIsValid(UsernameInput.text))
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    void Start()
    {
        
        RegisterButton.onClick.AddListener(() =>
        {
            if (CheckInputs()) 
            {
                StartCoroutine(CreateUser(UsernameInput.text, PasswordInput.text));
            }
        });
        

    }
}
