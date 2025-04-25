using Riptide;
using System.Collections;
using System.Collections.Generic;
using System.Net.Configuration;
using UnityEngine;

public class PowerUp : MonoBehaviour
{
    public enum PowerUpEffects
    {
        Dash,
        SprintBoost,
        Invicible,
        Healing,
        // Invisible
    }


    [SerializeField] private PowerUpEffects effects = PowerUpEffects.Dash;
    [SerializeField] private float duration;
    [SerializeField] private GameObject parentIcon;

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            PowerUpPickUp(other);
        }
    }

    private void PowerUpPickUp(Collider player)
    {
        if (player != null)
        {
            if (effects == PowerUpEffects.Dash || effects == PowerUpEffects.SprintBoost)
            {
                if (player.TryGetComponent<PlayerMovement>(out var movement))
                {
                    if (effects == PowerUpEffects.Dash)
                    {
                        movement.EnableDash = true;
                    }
                    else if (effects == PowerUpEffects.SprintBoost)
                    {
                        movement.ActivateBoostedSprintSpeed();
                    }

                }
            }
            else if (effects == PowerUpEffects.Invicible || effects == PowerUpEffects.Healing)
            {
                if (player.TryGetComponent<Player>(out var playerScript))
                {
                    if (effects == PowerUpEffects.Invicible)
                    { 
                        playerScript.IsInvincible = true; 
                    }
                    else if(effects == PowerUpEffects.Healing)
                    {
                        playerScript.LowerHealth(-50);
                    }
                        
                }
            }
        }
        
        // server and client side
        GetComponent<MeshRenderer>().enabled = false;
        parentIcon.GetComponent<MeshRenderer>().enabled = false;
        // server side
        GetComponent<Collider>().enabled = false;
        SendPickupToClients(false);
        StartCoroutine(PickupCooldown(player));
    }

    private IEnumerator PickupCooldown(Collider player)
    {
       
        yield return new WaitForSeconds(duration);

        GetComponent<MeshRenderer>().enabled = true;
        parentIcon.GetComponent<MeshRenderer>().enabled = true;
        GetComponent<Collider>().enabled = true;
        if (player != null)
        {
            if (effects == PowerUpEffects.Dash || effects == PowerUpEffects.SprintBoost)
            {
                if (player.TryGetComponent<PlayerMovement>(out var movement))
                {
                    if (effects == PowerUpEffects.Dash)
                    {
                        movement.EnableDash = false;
                    }
                    else if (effects == PowerUpEffects.SprintBoost)
                    {
                        movement.DeactivateBoostedSprintSpeed();
                    }
                }
            }
            else if (effects == PowerUpEffects.Invicible)
            {
                if (player.TryGetComponent<Player>(out var playerScript))
                {
                    playerScript.IsInvincible = false;
                }
            }
        }
        
        SendPickupToClients(true);

        //remove powerup particle effect
        //remove other effects
        //Destroy(gameObject);
    }

    private void SendPickupToClients(bool isExpired)
    {
        Message message = Message.Create(MessageSendMode.Reliable, ServerToClientId.powerUp);
        message.AddInt(SpawnDynamicObjects.Singleton.GetObjectIds()[parentIcon]);
        message.AddBool(isExpired);
        NetworkManager.Singleton.Server.SendToAll(message);

    }

}
