using UnityEngine;

public interface IInteractable
{
    string InteractionDescription { get; }
    bool CanInteract(GameObject interactor);
    void Interact(GameObject interactor);
}
