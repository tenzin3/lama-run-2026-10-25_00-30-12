using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public const string PREF_BEST_DISTANCE = "TenzinRun_BestDistance";

    [Header("Speed Settings")]
    [Tooltip("Starting running speed in units/sec.")]
    [SerializeField] private float _startSpeed = 12f;
    [Tooltip("Maximum running speed cap.")]
    [SerializeField] private float _maxSpeed = 26f;
    [Tooltip("Speed increase rate per second.")]
    [SerializeField] private float _speedIncreaseRate = 0.15f;

    [Header("Player Reference")]
    [SerializeField] private PlayerController _player;

    private float _currentSpeed;
    private int _coins;
    private int _distance;
    private int _bestDistance;
    private bool _isStarted;
    private bool _isPaused;
    private bool _isGameOver;
    private int _lastBowlMilestone = 0;

    public float StartSpeed => _startSpeed;
    public float MaxSpeed => _maxSpeed;
    public float CurrentSpeed => _currentSpeed;
    public int Coins => _coins;
    public int Distance => _distance;
    public int BestDistance => _bestDistance;
    public bool IsStarted => _isStarted;
    public bool IsPaused => _isPaused;
    public bool IsGameOver => _isGameOver;
    public bool IsPlaying => _isStarted && !_isPaused && !_isGameOver;

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

        Time.timeScale = 1f;
        _currentSpeed = _startSpeed;
        _isStarted = false;
        _isPaused = false;
        _isGameOver = false;
        _coins = 0;
        _distance = 0;
        _lastBowlMilestone = 0;

        _bestDistance = PlayerPrefs.GetInt(PREF_BEST_DISTANCE, 0);
    }

    private void Start()
    {
        if (_player == null)
        {
            _player = FindAnyObjectByType<PlayerController>();
        }

        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowStartScreen(_bestDistance);
        }
    }

    private void Update()
    {
        HandleKeyboardShortcuts();

        if (!IsPlaying) return;

        // Increase speed over time
        _currentSpeed = Mathf.Min(_maxSpeed, _currentSpeed + _speedIncreaseRate * Time.deltaTime);

        // Track distance
        if (_player != null)
        {
            _distance = Mathf.Max(0, Mathf.FloorToInt(_player.DistanceTraveled));
        }

        // Check 500m singing bowl milestone
        if (_distance >= 500 && _distance / 500 > _lastBowlMilestone)
        {
            _lastBowlMilestone = _distance / 500;
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySingingBowl();
            }
        }

        // Update HUD
        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateHUD(_distance, _coins, _currentSpeed);
        }
    }

    private void HandleKeyboardShortcuts()
    {
        var keyboard = Keyboard.current;
        if (keyboard == null) return;

        if (keyboard.pKey.wasPressedThisFrame || keyboard.escapeKey.wasPressedThisFrame)
        {
            if (_isStarted && !_isGameOver)
            {
                if (_isPaused) ResumeGame();
                else PauseGame();
            }
        }
    }

    public void StartGame()
    {
        if (_isStarted) return;

        _isStarted = true;
        _isPaused = false;
        Time.timeScale = 1f;

        if (MeditationFlight.Instance != null)
        {
            MeditationFlight.Instance.ResetRun();
        }

        if (UIManager.Instance != null)
        {
            UIManager.Instance.OnGameStarted();
            UIManager.Instance.UpdateHUD(_distance, _coins, _currentSpeed);
        }
    }

    public void PauseGame()
    {
        if (!_isStarted || _isGameOver || _isPaused) return;

        _isPaused = true;
        Time.timeScale = 0f;

        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowPausePanel(true);
        }
    }

    public void ResumeGame()
    {
        if (!_isPaused) return;

        _isPaused = false;
        Time.timeScale = 1f;

        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowPausePanel(false);
        }
    }

    public void AddCoins(int amount)
    {
        if (_isGameOver) return;

        _coins += amount;

        if (MeditationFlight.Instance != null)
        {
            MeditationFlight.Instance.OnLampCollected(amount);
        }

        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateHUD(_distance, _coins, _currentSpeed);
        }
    }

    public void GameOver()
    {
        if (_isGameOver) return;

        _isGameOver = true;

        bool isNewBest = false;
        if (_distance > _bestDistance)
        {
            _bestDistance = _distance;
            PlayerPrefs.SetInt(PREF_BEST_DISTANCE, _bestDistance);
            PlayerPrefs.Save();
            isNewBest = true;
        }

        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowGameOver(_distance, _coins, _bestDistance, isNewBest);
        }
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void ReturnToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
