using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


public class GrabbingController : MonoBehaviour
{
    [SerializeField] private GameEvents _gameEvents;
    [SerializeField] private float _grabForce = 150f;

    private IGrabbable _grabedGrabbable;

    private void Awake()
    {
        _gameEvents.OnTheftComplete += DisableController;
        _gameEvents.OnTheftFailed += DisableController;
    }

    private void OnDestroy()
    {
        _gameEvents.OnTheftComplete -= DisableController;
        _gameEvents.OnTheftFailed -= DisableController;
    }

    private void FixedUpdate()
    {
        if (_grabedGrabbable != null && _grabedGrabbable.IsGrabbed)
        {
            if (Vector3.Distance(_grabedGrabbable.Transform.position, transform.position) > 0.1f)
            {
                Vector3 moveDirection = (transform.position - _grabedGrabbable.Transform.position).normalized;
                _grabedGrabbable.Rb.AddForce(moveDirection * _grabForce);
            }
        }
    }
    private void DisableController()
    {
        if (_grabedGrabbable != null && _grabedGrabbable.IsGrabbed)
        {
            Drop(_grabedGrabbable);
        }
        enabled = false;
    }
    public void Grab(IGrabbable grabbable)
    {
        if (_grabedGrabbable != null && _grabedGrabbable != grabbable)
        {
            Drop(_grabedGrabbable);
        }
        _grabedGrabbable = grabbable;
        _grabedGrabbable.Rb.transform.parent = transform;
        _grabedGrabbable.Grab();
        _gameEvents.OnInteractableUpdate?.Invoke(grabbable.Interactable);
    }

    public void Drop(IGrabbable grabbable)
    {
        if (grabbable != _grabedGrabbable)
            return;

        grabbable.Drop();
        _gameEvents.OnInteractableUpdate?.Invoke(grabbable.Interactable);
        _grabedGrabbable = null;
    }

}