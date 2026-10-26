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
    [SerializeField] private Button _hudMuteButton;
    [SerializeField] private TextMeshProUGUI _hudMuteText;
    [SerializeField] private TextMeshProUGUI _controlsHintText;

    [Header("Chaser Hint")]
    [SerializeField] private GameObject _chaserHintPanel;
    [SerializeField] private TextMeshProUGUI _chaserHintText;

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

    private bool _hasShownLangurHint = false;
    private Coroutine _chaserHintCoroutine;

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

        if (_hudMuteButton != null)
        {
            _hudMuteButton.onClick.AddListener(OnMuteButtonClicked);
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

    private void Start()
    {
        if (_controlsHintText == null && _hudPanel != null)
        {
            var hint = _hudPanel.transform.Find("ControlsHint");
            if (hint != null)
            {
                _controlsHintText = hint.GetComponent<TextMeshProUGUI>();
            }
        }

        // Apply safe area component to HUD panel if missing
        if (_hudPanel != null && _hudPanel.GetComponent<SafeArea>() == null)
        {
            _hudPanel.AddComponent<SafeArea>();
        }

        if (IsTouchDevice())
        {
            SetTouchControlsHint();
        }
        else
        {
            SetKeyboardControlsHint();
        }

        UpdateMuteButtonText();
    }

    public static bool IsTouchDevice()
    {
        if (Application.isMobilePlatform || SystemInfo.deviceType == DeviceType.Handheld)
        {
            return true;
        }

        if (UnityEngine.InputSystem.Touchscreen.current != null)
        {
            return true;
        }

        return false;
    }

    public void SetTouchControlsHint()
    {
        if (_controlsHintText != null)
        {
            _controlsHintText.text = "Swipe to move · Swipe up to jump · Swipe down to slide";
        }

        if (_startPromptText != null)
        {
            _startPromptText.text = "Tap to run";
        }
    }

    public void SetKeyboardControlsHint()
    {
        if (_controlsHintText != null)
        {
            _controlsHintText.text = "A/D or Arrows / Swipe: Change Lane   |   W/Space / Swipe Up: Jump   |   S / Swipe Down: Slide";
        }

        if (_startPromptText != null)
        {
            _startPromptText.text = "Tap or press Space to run";
        }
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

    public void ShowLangurHint()
    {
        if (_hasShownLangurHint) return;
        _hasShownLangurHint = true;

        EnsureChaserHintUI();
        if (_chaserHintPanel != null)
        {
            if (_chaserHintCoroutine != null) StopCoroutine(_chaserHintCoroutine);
            _chaserHintCoroutine = StartCoroutine(DisplayChaserHintRoutine());
        }
    }

    private void EnsureChaserHintUI()
    {
        if (_chaserHintPanel != null && _chaserHintText != null) return;

        if (_hudPanel != null)
        {
            var existing = _hudPanel.transform.Find("ChaserHintPanel");
            if (existing != null)
            {
                _chaserHintPanel = existing.gameObject;
                _chaserHintText = _chaserHintPanel.GetComponentInChildren<TextMeshProUGUI>();
            }
            else
            {
                GameObject banner = new GameObject("ChaserHintPanel", typeof(RectTransform), typeof(CanvasGroup));
                banner.transform.SetParent(_hudPanel.transform, false);

                RectTransform rt = banner.GetComponent<RectTransform>();
                rt.anchorMin = new Vector2(0.5f, 1f);
                rt.anchorMax = new Vector2(0.5f, 1f);
                rt.pivot = new Vector2(0.5f, 1f);
                rt.anchoredPosition = new Vector2(0f, -115f);
                rt.sizeDelta = new Vector2(460f, 48f);

                var img = banner.AddComponent<Image>();
                img.color = new Color(0.14f, 0.05f, 0.05f, 0.85f);

                GameObject textObj = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
                textObj.transform.SetParent(banner.transform, false);
                RectTransform textRt = textObj.GetComponent<RectTransform>();
                textRt.anchorMin = Vector2.zero;
                textRt.anchorMax = Vector2.one;
                textRt.sizeDelta = Vector2.zero;

                var tmp = textObj.GetComponent<TextMeshProUGUI>();
                if (_speedText != null && _speedText.font != null)
                {
                    tmp.font = _speedText.font;
                }
                tmp.text = "The langur is right behind you!";
                tmp.fontSize = 20f;
                tmp.fontStyle = FontStyles.Bold;
                tmp.alignment = TextAlignmentOptions.Center;
                tmp.color = new Color(1.0f, 0.82f, 0.28f, 1.0f);

                _chaserHintPanel = banner;
                _chaserHintText = tmp;
                _chaserHintPanel.SetActive(false);
            }
        }
    }

    private System.Collections.IEnumerator DisplayChaserHintRoutine()
    {
        if (_chaserHintPanel == null) yield break;

        _chaserHintPanel.SetActive(true);
        CanvasGroup cg = _chaserHintPanel.GetComponent<CanvasGroup>();
        if (cg == null) cg = _chaserHintPanel.AddComponent<CanvasGroup>();

        cg.alpha = 0f;
        float elapsed = 0f;
        while (elapsed < 0.25f)
        {
            elapsed += Time.deltaTime;
            cg.alpha = Mathf.Clamp01(elapsed / 0.25f);
            yield return null;
        }
        cg.alpha = 1f;

        yield return new WaitForSeconds(2.6f);

        elapsed = 0f;
        while (elapsed < 0.4f)
        {
            elapsed += Time.deltaTime;
            cg.alpha = 1f - Mathf.Clamp01(elapsed / 0.4f);
            yield return null;
        }

        _chaserHintPanel.SetActive(false);
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
        if (AudioManager.Instance != null)
        {
            bool muted = AudioManager.Instance.IsMuted;
            if (_startScreenMuteText != null)
            {
                _startScreenMuteText.text = muted ? "SOUND: OFF" : "SOUND: ON";
            }
            if (_hudMuteText != null)
            {
                _hudMuteText.text = muted ? "OFF" : "SFX";
            }
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
