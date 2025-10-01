using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TheftController : MonoBehaviour
{
    [SerializeField] GameEvents _gameEvents;

    Dictionary<IInteractable, int> _itemsToSteal = new Dictionary<IInteractable, int>();
    List<IInteractable> _stolenItems = new List<IInteractable>();


    private void Awake()
    {
        _gameEvents.OnGameRestart += RestartGame;
    }
    private void OnDestroy()
    {
        _gameEvents.OnGameRestart -= RestartGame;
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
  
    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent(out IInteractable interactable))
        {
            if (_itemsToSteal.ContainsKey(interactable))
            {
                _itemsToSteal[interactable] -= 1;
                if (_itemsToSteal[interactable] == 0)
                {
                    _stolenItems.Add(interactable);
                    _gameEvents.OnUpdateScore?.Invoke(_stolenItems.Count, _itemsToSteal.Count);
                    if (_stolenItems.Count == _itemsToSteal.Count)
                    {
                        _gameEvents.OnTheftComplete?.Invoke();
                    }
                }
            }
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out IInteractable interactable))
        {
            if (_itemsToSteal.ContainsKey(interactable))
            {
                _itemsToSteal[interactable] += 1;
            }
            else
            {
                _itemsToSteal.Add(interactable, 1);
            }
            if (_stolenItems.Contains(interactable))
            {
                _stolenItems.Remove(interactable);
            }
            _gameEvents.OnUpdateScore?.Invoke(_stolenItems.Count, _itemsToSteal.Count);
        }
    }
}
