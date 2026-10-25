using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("Start Screen")]
    [SerializeField] private GameObject _startScreenPanel;
    [SerializeField] private TextMeshProUGUI _startTitleText;
    [SerializeField] private TextMeshProUGUI _startPromptText;
    [SerializeField] private TextMeshProUGUI _startBestDistanceText;
    [SerializeField] private Button _startScreenMuteButton;
    [SerializeField] private TextMeshProUGUI _startScreenMuteText;
    [SerializeField] private Button _startScreenClickCatcher;

    [Header("HUD Elements")]
    [SerializeField] private GameObject _hudPanel;
    [SerializeField] private TextMeshProUGUI _distanceText;
    [SerializeField] private TextMeshProUGUI _coinsText;
    [SerializeField] private TextMeshProUGUI _speedText;
    [SerializeField] private Button _pauseButton;

    [Header("Pause Screen")]
    [SerializeField] private GameObject _pausePanel;
    [SerializeField] private Button _resumeButton;
    [SerializeField] private Button _pauseRestartButton;

    [Header("Game Over Screen")]
    [SerializeField] private GameObject _gameOverPanel;
    [SerializeField] private TextMeshProUGUI _finalDistanceText;
    [SerializeField] private TextMeshProUGUI _finalCoinsText;
    [SerializeField] private TextMeshProUGUI _finalBestDistanceText;
    [SerializeField] private GameObject _newBestBadge;
    [SerializeField] private Button _restartButton;
    [SerializeField] private Button _mainMenuButton;

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

        // Setup Button Listeners
        if (_startScreenClickCatcher != null)
        {
            _startScreenClickCatcher.onClick.AddListener(OnStartCatcherClicked);
        }

        if (_startScreenMuteButton != null)
        {
            _startScreenMuteButton.onClick.AddListener(OnMuteButtonClicked);
        }

        if (_pauseButton != null)
        {
            _pauseButton.onClick.AddListener(OnPauseClicked);
        }

        if (_resumeButton != null)
        {
            _resumeButton.onClick.AddListener(OnResumeClicked);
        }

        if (_pauseRestartButton != null)
        {
            _pauseRestartButton.onClick.AddListener(OnRestartClicked);
        }

        if (_restartButton != null)
        {
            _restartButton.onClick.AddListener(OnRestartClicked);
        }

        if (_mainMenuButton != null)
        {
            _mainMenuButton.onClick.AddListener(OnMainMenuClicked);
        }

        // Initial state
        if (_pausePanel != null) _pausePanel.SetActive(false);
        if (_gameOverPanel != null) _gameOverPanel.SetActive(false);
    }

    public void ShowStartScreen(int bestDistance)
    {
        if (_startScreenPanel != null) _startScreenPanel.SetActive(true);
        if (_hudPanel != null) _hudPanel.SetActive(false);
        if (_pausePanel != null) _pausePanel.SetActive(false);
        if (_gameOverPanel != null) _gameOverPanel.SetActive(false);

        if (_startBestDistanceText != null)
        {
            _startBestDistanceText.text = $"Best Distance: {bestDistance} m";
        }

        UpdateMuteButtonText();
    }

    public void OnGameStarted()
    {
        if (_startScreenPanel != null) _startScreenPanel.SetActive(false);
        if (_hudPanel != null) _hudPanel.SetActive(true);
        if (_pausePanel != null) _pausePanel.SetActive(false);
        if (_gameOverPanel != null) _gameOverPanel.SetActive(false);
    }

    public void ShowPausePanel(bool show)
    {
        if (_pausePanel != null)
        {
            _pausePanel.SetActive(show);
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
            _coinsText.text = $"Lamps: {coins}";
        }

        if (_speedText != null)
        {
            _speedText.text = $"{speed:F1} m/s";
        }
    }

    public void ShowGameOver(int distance, int coins, int bestDistance, bool isNewBest)
    {
        if (_pausePanel != null) _pausePanel.SetActive(false);
        if (_gameOverPanel != null) _gameOverPanel.SetActive(true);

        if (_finalDistanceText != null)
        {
            _finalDistanceText.text = $"Distance: {distance} m";
        }

        if (_finalCoinsText != null)
        {
            _finalCoinsText.text = $"Butter Lamps: {coins}";
        }

        if (_finalBestDistanceText != null)
        {
            _finalBestDistanceText.text = $"Best Distance: {bestDistance} m";
        }

        if (_newBestBadge != null)
        {
            _newBestBadge.SetActive(isNewBest);
        }
    }

    private void OnStartCatcherClicked()
    {
        if (GameManager.Instance != null && !GameManager.Instance.IsStarted)
        {
            GameManager.Instance.StartGame();
        }
    }

    private void OnMuteButtonClicked()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.ToggleMute();
            UpdateMuteButtonText();
        }
    }

    private void UpdateMuteButtonText()
    {
        if (_startScreenMuteText != null && AudioManager.Instance != null)
        {
            _startScreenMuteText.text = AudioManager.Instance.IsMuted ? "SOUND: OFF" : "SOUND: ON";
        }
    }

    private void OnPauseClicked()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.PauseGame();
        }
    }

    private void OnResumeClicked()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ResumeGame();
        }
    }

    private void OnRestartClicked()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.RestartGame();
        }
    }

    private void OnMainMenuClicked()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ReturnToMainMenu();
        }
    }
}
