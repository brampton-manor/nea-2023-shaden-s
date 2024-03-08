using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArmourInteractable : MonoBehaviour
{
    [SerializeField] AudioClip ArmourSound;
    AudioSource audioSource;

    [SerializeField] string name;
    [SerializeField] int requiredPoints;
    [SerializeField] int armourAmount;

    public void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }
    public void HandleUI(PlayerController player)
    {
        player.SetInteractUI(name + " - " + requiredPoints + "  points", "Press E to equip Armour");
    }

    public void EquipArmour(PlayerController player)
    {
        if (player.currentPoints < requiredPoints) player.PoorEnable();
        else if (player.currentArmour == player.maxArmour) player.AlreadyMaxHealth();
        else
        {
            if (player.currentArmour + armourAmount >= player.maxArmour) player.currentArmour = player.maxArmour;
            else player.currentArmour += armourAmount;
            audioSource.PlayOneShot(ArmourSound, 0.9f);
            player.SpendPoints(requiredPoints);
        }
    }
}
