using System.Collections.Generic;
using UnityEngine;

public class InteractionController : MonoBehaviour
{
    [SerializeField] GameEvents _gameEvents;
    [SerializeField] GameObject _playerObject;

    public float interactionDistance = 3f;
    public KeyCode interactKey = KeyCode.E;
    public LayerMask interactionLayer;

    Camera _playerCamera;
    string _interactionPrefix = "Press";
    List<IInteractable> _detectedInteractables = new List<IInteractable>();


    private void Awake()
    {
        _playerCamera = _playerObject.GetComponentInChildren<Camera>();
        _gameEvents.OnTheftComplete += DisableController;
        _gameEvents.OnTheftFailed += DisableController;
        _gameEvents.OnInteractableUpdate += FormatInteractionHint;
        ConfigureTrigger();
    }

    private void ConfigureTrigger()
    {
        BoxCollider boxCollider = GetComponent<BoxCollider>();
        if (boxCollider == null)
        {
            boxCollider = gameObject.AddComponent<BoxCollider>();
            boxCollider.isTrigger = true;
        }
        boxCollider.size = new Vector3(0.1f, 0.1f, interactionDistance);
        boxCollider.center = new Vector3(0.25f, -0.25f, interactionDistance / 2 - transform.localPosition.z);
    }

    private void OnDestroy()
    {
        _gameEvents.OnTheftComplete -= DisableController;
        _gameEvents.OnTheftFailed -= DisableController;
        _gameEvents.OnInteractableUpdate -= FormatInteractionHint;
    }

    private void DisableController()
    {
        enabled = false;
    }
    private void Update()
    {
        if (Input.GetKeyDown(interactKey))
        {
            TryInteract();
        }
    }

    private void TryInteract()
    {
        Ray ray = new Ray(_playerCamera.transform.position, _playerCamera.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, interactionDistance, interactionLayer))
        {
            IInteractable interactable = hit.collider.GetComponent<IInteractable>();
            if (interactable != null)
            {
                interactable.Interact(_playerObject);
            }
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out IInteractable interactable))
        {
            _detectedInteractables.Add(interactable);
            _gameEvents.OnInteractableUpdate?.Invoke(interactable);
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent(out IInteractable interactable))
        {
            _detectedInteractables.Remove(interactable);
            if (_detectedInteractables.Count == 0)
            {
                _gameEvents.OnInteractablesNotFound?.Invoke();
            }
            else
            {
                _gameEvents.OnInteractableUpdate?.Invoke(_detectedInteractables[^1]);
            }
        }
    }
    private void FormatInteractionHint(IInteractable interactable)
    {
        string hint = $"{_interactionPrefix} {interactKey} {interactable.GetInteractionHint()}";
        _gameEvents.OnInteractableHintUpdate?.Invoke(hint);
    }
}