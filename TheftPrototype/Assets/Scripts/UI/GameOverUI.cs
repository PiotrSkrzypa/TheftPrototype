using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class GameOverUI : MonoBehaviour
{
    [SerializeField] GameEvents _gameEvents;
    [SerializeField] float _gameOverDelay = 1f;
    [SerializeField] GameObject _wonPanel;
    [SerializeField] GameObject _lostPanel;
    private void Awake()
    {
        _gameEvents.OnTheftComplete += HandleTheftComplete;
        _gameEvents.OnTheftFailed += HandleTheftFailed;
        _wonPanel.GetComponentInChildren<Button>().onClick.AddListener(RestartGame);
        _lostPanel.GetComponentInChildren<Button>().onClick.AddListener(RestartGame);
    }
    private void RestartGame()
    {
        _gameEvents.OnGameRestart.Invoke();
    }
    private void OnDestroy()
    {
        _gameEvents.OnTheftComplete -= HandleTheftComplete;
        _gameEvents.OnTheftFailed -= HandleTheftFailed;
    }
    private void HandleTheftComplete()
    {
        StartCoroutine(WinCoroutine());
    }

    private void HandleTheftFailed()
    {
        StartCoroutine(LooseCoroutine());
    }

    IEnumerator WinCoroutine()
    {
        yield return new WaitForSeconds(_gameOverDelay);
        _wonPanel.SetActive(true);
        _lostPanel.SetActive(false);
    }
    IEnumerator LooseCoroutine()
    {
        yield return new WaitForSeconds(_gameOverDelay);
        _lostPanel.SetActive(true);
        _wonPanel.SetActive(false);
    }
}
