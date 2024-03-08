using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Ladder : MonoBehaviour
{
    public void HandleUI(PlayerController player)
    {
        if(!player.isClimbing) player.SetInteractUI("Ladder", "Press E to climb");
    }

}
