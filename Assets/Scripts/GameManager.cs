using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

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
    private bool _isGameOver;

    public float CurrentSpeed => _currentSpeed;
    public int Coins => _coins;
    public int Distance => _distance;
    public bool IsGameOver => _isGameOver;

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

        _currentSpeed = _startSpeed;
        _isGameOver = false;
        _coins = 0;
        _distance = 0;
    }

    private void Start()
    {
        if (_player == null)
        {
            _player = FindAnyObjectByType<PlayerController>();
        }
    }

    private void Update()
    {
        if (_isGameOver) return;

        // Increase speed over time
        _currentSpeed = Mathf.Min(_maxSpeed, _currentSpeed + _speedIncreaseRate * Time.deltaTime);

        // Track distance
        if (_player != null)
        {
            _distance = Mathf.Max(0, Mathf.FloorToInt(_player.DistanceTraveled));
        }

        // Update HUD
        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateHUD(_distance, _coins, _currentSpeed);
        }
    }

    public void AddCoins(int amount)
    {
        if (_isGameOver) return;

        _coins += amount;
    }

    public void GameOver()
    {
        if (_isGameOver) return;

        _isGameOver = true;

        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowGameOver(_distance, _coins);
        }
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
