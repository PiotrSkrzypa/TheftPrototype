using TMPro;
using UnityEngine;


public class InteractionHintUI : MonoBehaviour
{
    [SerializeField] GameEvents _gameEvents;
    [SerializeField] Canvas _canvas;
    [SerializeField] TextMeshProUGUI _hintText;
    private void Awake()
    {
        if (_canvas == null)
        {
            _canvas = GetComponentInChildren<Canvas>();
        }
        SubscribeGameEvents();
    }

    private void SubscribeGameEvents()
    {
        _gameEvents.OnInteractableHintUpdate += UpdateHintText;
        _gameEvents.OnInteractablesNotFound += DisableHintText;
        _gameEvents.OnTheftFailed += DisableAndUnsubscribe;
        _gameEvents.OnTheftComplete += DisableAndUnsubscribe;
    }

    private void OnDestroy()
    {
        UnsubscribeGameEvents();
    }

    private void UnsubscribeGameEvents()
    {
        _gameEvents.OnInteractableHintUpdate -= UpdateHintText;
        _gameEvents.OnInteractablesNotFound -= DisableHintText;
        _gameEvents.OnTheftFailed -= DisableAndUnsubscribe;
        _gameEvents.OnTheftComplete -= DisableAndUnsubscribe;
    }

    private void UpdateHintText(string message)
    {
        if (_canvas.enabled == false)
        {
            _canvas.enabled = true;
        }
        _hintText.text = message;
    }
    private void DisableHintText()
    {
        _hintText.text = string.Empty;
        _canvas.enabled = false;
    }
    private void DisableAndUnsubscribe()
    {
        DisableHintText();
        UnsubscribeGameEvents();
    }

}
