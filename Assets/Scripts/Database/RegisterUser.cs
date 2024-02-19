using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using System.Linq;
using System;

public class RegisterUser : MonoBehaviour
{
    public TMP_InputField UsernameInput;
    public TMP_InputField PasswordInput;
    public TMP_Text DebugText;
    public Button RegisterButton;

    bool Valid = false;

    // Coroutine to create a user
    public IEnumerator CreateUser(string username, string password)
    {
        // Create a new form to send data
        WWWForm form = new WWWForm();
        form.AddField("newUser", username);
        form.AddField("newPass", password);

        // Send a UnityWebRequest to the server
        using (UnityWebRequest www = UnityWebRequest.Post("http://localhost/NEA/RegisterUser.php", form))
        {
            yield return www.SendWebRequest();

            // Check if the request was successful
            if (www.result != UnityWebRequest.Result.Success)
            {
                DebugText.text = www.error;
            }
            else
            {
                DebugText.text = www.downloadHandler.text;
            }
        }
    }

    // Check if password contains at least one lowercase character
    bool CheckIfLowerCase(string password)
    {
        if (password.Any(char.IsLower)) return true;
        DebugText.text = "Password must contain at least 1 lowercase character\n";
        return false;
    }

    // Check if password contains at least one uppercase character
    bool CheckIfUpperCase(string password)
    {
        if (password.Any(char.IsUpper)) return true;
        DebugText.text = "Password must contain at least 1 uppercase character\n";
        return false;
    }

    // Check if password contains at least one digit
    bool CheckIfDigit(string password)
    {
        if (password.Any(char.IsDigit)) return true;
        DebugText.text = "Password must contain at least 1 digit\n";
        return false;
    }

    // Check if password contains at least one symbol
    bool CheckIfSymbol(string password)
    {
        if (password.Any(char.IsSymbol) || password.Any(char.IsPunctuation)) return true;
        DebugText.text = "Password must contain at least 1 special character\n";
        return false;
    }

    // Check if password length is between 8 and 40 characters
    bool CheckPasswordLength(string password)
    {
        if (password.Length >= 8 && password.Length <= 40) return true;
        DebugText.text = "Password must be between 8 - 40 characters\n";
        return false;
    }

    // Validate password
    bool PasswordIsValid(string password)
    {
        return CheckIfLowerCase(password) &&
               CheckIfUpperCase(password) &&
               CheckIfDigit(password) &&
               CheckIfSymbol(password) &&
               CheckPasswordLength(password);
    }

    // Check if username length is between 4 and 20 characters
    bool CheckUsernameLength(string username)
    {
        if (username.Length >= 4 && username.Length <= 20) return true;
        DebugText.text = "Username must be between 4 - 20 characters\n";
        return false;
    }

    // Validate username
    bool UsernameIsValid(string username)
    {
        if (!username.All(char.IsLetterOrDigit))
        {
            DebugText.text = "Username must be alphanumeric\n";
            return false;
        }
        return CheckUsernameLength(username);
    }

    // Check inputs for both username and password validity
    public bool CheckInputs()
    {
        return UsernameIsValid(UsernameInput.text) && PasswordIsValid(PasswordInput.text);
    }

    void Start()
    {
        // Add a listener to the register button
        RegisterButton.onClick.AddListener(() =>
        {
            if (CheckInputs())
            {
                StartCoroutine(CreateUser(UsernameInput.text, PasswordInput.text));
            }
        });
    }
}
