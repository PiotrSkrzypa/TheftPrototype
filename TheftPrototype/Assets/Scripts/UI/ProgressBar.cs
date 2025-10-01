using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class ProgressBar : MonoBehaviour
{
    [SerializeField] GameEvents _gameEvents;
    [SerializeField] Image _fillImage;
    [SerializeField] TextMeshProUGUI _progressText;

    private void Awake()
    {
        if (_gameEvents != null)
        {
            _gameEvents.OnUpdateScore += OnProgressChanged;
        }
    }
    private void OnDestroy()
    {
        if (_gameEvents != null)
        {
            _gameEvents.OnUpdateScore -= OnProgressChanged;
        }
    }
    void OnProgressChanged(int currentValue, int maxValue)
    {
        if (maxValue > 0)
        {
            _fillImage.fillAmount = (float)currentValue / maxValue;
        }
        _progressText.text = $"{currentValue}/{maxValue}";
    }
}
