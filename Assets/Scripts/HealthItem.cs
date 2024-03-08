using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthItem : MonoBehaviour
{
    [SerializeField] AudioClip HealSound;
    AudioSource audioSource;

    [SerializeField] string name;
    [SerializeField] int requiredPoints;
    [SerializeField] int healAmount;


    public void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }
    public void HandleUI(PlayerController player)
    {
        player.SetInteractUI(name + " - " + requiredPoints + "  points", "Press E to heal");
    }
    
    public void HealPlayer(PlayerController player)
    {
        if (player.currentPoints < requiredPoints) player.PoorEnable();
        else if (player.currentHealth == player.maxHealth) player.AlreadyMaxHealth();
        else
        {
            if (player.currentHealth + healAmount >= player.maxHealth) player.currentHealth = player.maxHealth;
            else player.currentHealth += healAmount;
            audioSource.PlayOneShot(HealSound, 0.9f);
            player.SpendPoints(requiredPoints);
        }
    }
}
