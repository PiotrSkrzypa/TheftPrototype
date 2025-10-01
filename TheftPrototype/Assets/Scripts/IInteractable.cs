using UnityEngine;

public interface IInteractable
{
    void Interact(GameObject interactionInitiator);
    string GetInteractionHint();
}