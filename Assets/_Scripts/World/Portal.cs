using UnityEngine;

public class Portal : MonoBehaviour
{
    public Portal destinationPortal;

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Check if the colliding object is the player
        if (other.CompareTag("Player"))
        {
            if (!other.gameObject.GetComponent<PlayerController>().teleporting)
            {
                other.gameObject.GetComponent<PlayerController>().teleporting = true;
                SoundManager.Post(SoundManager.Sound.Portal);
                // Teleport the player to the destination portal
                TeleportPlayer(other.gameObject);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        // Check if the colliding object is the player
        if (other.CompareTag("Player"))
        {
            other.gameObject.GetComponent<PlayerController>().teleporting = false;
        }
    }

    private void TeleportPlayer(GameObject player)
    {
        if(player.GetComponent<PlayerController>().teleporting)
        // Ensure there is a destination portal assigned
        if (destinationPortal != null)
        {
            // Move the player to the destination portal's position
            player.transform.position = destinationPortal.transform.position;
        }
        else
        {
            Debug.LogWarning("Destination portal is not assigned for " + gameObject.name);
        }
    }
}
