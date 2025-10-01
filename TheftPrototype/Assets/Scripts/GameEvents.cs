using System.Collections;
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "GameEvents", menuName = "ScriptableObjects/GameEvents", order = 1)]
public class GameEvents : ScriptableObject
{
    public UnityAction<int,int> OnUpdateScore;
    public UnityAction<IInteractable> OnInteractableUpdate;
    public UnityAction<string> OnInteractableHintUpdate;
    public UnityAction OnInteractablesNotFound;
    public UnityAction OnTheftComplete;
    public UnityAction OnTheftFailed;
    public UnityAction OnGameRestart;
}
