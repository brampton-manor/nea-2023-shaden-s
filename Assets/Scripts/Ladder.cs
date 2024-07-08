using UnityEngine;

public class Ladder : MonoBehaviour
{
    public void HandleUI(PlayerController player)
    {
        if(!player.isClimbing) player.SetInteractUI("Ladder", "Press E to climb");
    }

}
