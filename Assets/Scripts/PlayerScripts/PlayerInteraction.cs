using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    private PlayerController playerController;
    private IInteractable CurrentInteractableObject = null;


    private void Awake()
    {
        playerController = GetComponent<PlayerController>();
    }

    // Update is called once per frame
    void Update()
    {
        if (playerController.Controls.Interaction.Enter.WasPressedThisFrame())
        {
            CurrentInteractableObject?.Interact();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        CurrentInteractableObject = collision.GetComponent<IInteractable>();
    }

    private void OnTriggerExit2D(Collider2D collision)
    {

        CurrentInteractableObject = null;
    }
}
