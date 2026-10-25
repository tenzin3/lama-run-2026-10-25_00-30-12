using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("HUD Elements")]
    [SerializeField] private TextMeshProUGUI _distanceText;
    [SerializeField] private TextMeshProUGUI _coinsText;
    [SerializeField] private TextMeshProUGUI _speedText;

    [Header("Game Over Screen")]
    [SerializeField] private GameObject _gameOverPanel;
    [SerializeField] private TextMeshProUGUI _finalDistanceText;
    [SerializeField] private TextMeshProUGUI _finalCoinsText;
    [SerializeField] private Button _restartButton;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        if (_restartButton != null)
        {
            _restartButton.onClick.AddListener(OnRestartClicked);
        }

        if (_gameOverPanel != null)
        {
            _gameOverPanel.SetActive(false);
        }
    }

    public void UpdateHUD(int distance, int coins, float speed)
    {
        if (_distanceText != null)
        {
            _distanceText.text = $"{distance} m";
        }

        if (_coinsText != null)
        {
            _coinsText.text = $"Coins: {coins}";
        }

        if (_speedText != null)
        {
            _speedText.text = $"{speed:F1} m/s";
        }
    }

    public void ShowGameOver(int distance, int coins)
    {
        if (_gameOverPanel != null)
        {
            _gameOverPanel.SetActive(true);
        }

        if (_finalDistanceText != null)
        {
            _finalDistanceText.text = $"Distance: {distance} m";
        }

        if (_finalCoinsText != null)
        {
            _finalCoinsText.text = $"Coins: {coins}";
        }
    }

    private void OnRestartClicked()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.RestartGame();
        }
    }
}
