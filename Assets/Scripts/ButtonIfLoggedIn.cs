using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonIfLoggedIn : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private bool requiresLogIn;
    [SerializeField] private bool hideWhenLoggedIn;

    void Start()
    {
        button.gameObject.SetActive(true);
    }
    void Update()
    {
        if (Login.Instance.LoggedIn)
        {
            if(!hideWhenLoggedIn)button.gameObject.SetActive(true);
            else button.gameObject.SetActive(false);
        }
        else
        {
            if (requiresLogIn) button.gameObject.SetActive(false);
            else button.gameObject.SetActive(true);
        }
    }
}
