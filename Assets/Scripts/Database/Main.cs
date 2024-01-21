using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Main : MonoBehaviour
{

    public static Main Instance;

    public Web Web;

    public bool Log;

    void Start()
    {
        Instance = this;
        Web = GetComponent<Web>();
    }

}
