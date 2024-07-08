using UnityEngine;

public class WeaponInteractable : MonoBehaviour
{
    public string InteractableName;
    public int requiredPoints;
    public int itemIndex;
    public void AddToInventory(PlayerController player)
    {
        if (player.currentPoints < requiredPoints) player.PoorEnable();
        else if (player.items[itemIndex].isAbleToBeUsed) player.AlreadyInInventoryEnable();
        else
        {
            player.SpendPoints(requiredPoints);
            player.BoughtEnable();
            player.items[itemIndex].isAbleToBeUsed = true;
        }
    }

    public void HandleUI(PlayerController player)
    {
        player.SetBuyInteractUI(InteractableName, requiredPoints);
    }
}
