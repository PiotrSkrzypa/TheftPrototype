using UnityEngine;

public interface IGrabbable
{
    Rigidbody Rb { get; }
    Transform Transform { get; }
    IInteractable Interactable { get; }
    bool IsGrabbed { get;}
    void Grab();
    void Drop();
}