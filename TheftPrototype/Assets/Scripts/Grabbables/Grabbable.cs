using UnityEngine;
using UnityEngine.Events;


public class Grabbable : MonoBehaviour, IInteractable, IGrabbable
{
    [SerializeField] private Rigidbody _rb;
    [SerializeField] private UnityEvent _onGrabbed;
    [SerializeField] private UnityEvent _onDropped;
    private GrabbingController _grabbingController;
    private Transform _originalParent;
    private bool _isGrabbed = false;

    public Rigidbody Rb { get => _rb; }
    public Transform Transform { get => transform; }
    public IInteractable Interactable => this;
    public bool IsGrabbed { get => _isGrabbed; }

    private void Awake()
    {
        if (_rb == null)
        {
            _rb = GetComponentInChildren<Rigidbody>();
        }
        _originalParent = transform.parent;
    }
    public void Interact(GameObject interactionInitiator)
    {
        _grabbingController = interactionInitiator.GetComponentInChildren<GrabbingController>();
        if (_grabbingController == null)
        {
            Debug.LogWarning("GrabbingController not found on interaction initiator. Ensure it is attached to the correct GameObject.");
            return;
        }
        if (_isGrabbed)
        {
            _grabbingController.Drop(this);
        }
        else
        {
            _grabbingController.Grab(this);
        }
    }

    public void Grab()
    {
        _rb.useGravity = false;
        _rb.constraints = RigidbodyConstraints.FreezeRotation; // Prevent rotation while grabbed
        _rb.linearDamping = 15f; // Optional: Adjust damping for smoother movement
        _onGrabbed?.Invoke();
        _isGrabbed = true;
    }
    public void Drop()
    {
        _grabbingController = null;
        _rb.transform.parent = _originalParent;
        _rb.useGravity = true;
        _rb.constraints = RigidbodyConstraints.None;
        _rb.linearDamping = 0f;
        _onDropped?.Invoke();
        _isGrabbed = false;
    }

    public string GetInteractionHint()
    {
        if (!_isGrabbed)
        {
            return "to pick up";
        }
        else
        {
            return "to drop";
        }
    }
}
