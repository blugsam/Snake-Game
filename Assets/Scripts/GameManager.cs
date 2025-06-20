using TMPro;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class GameManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Board board;
    [SerializeField] private Snake snake;
    [SerializeField] private RectTransform boardPanelRect;

    [Header("UI Panels")]
    [SerializeField] private GameObject menuPanel;
    [SerializeField] private GameObject confirmationPanel;

    [Header("UI Text")]
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text highScoreText;
    [SerializeField] private TMP_Text menuMessage;

    [Header("Audio")]
    [SerializeField] private AudioClip biteSoundClip;
    private AudioSource _audioSource;

    private int _score;
    private int _highScore;
    private bool _isGameActive = false;

    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();

        snake.OnAteFood += HandleFoodEaten;
        snake.OnDied += HandleGameOver;
    }

    private void Start()
    {
        ShowMenu(true);
    }

    public void OnPlayButton()
    {
        snake.ResetSnake();
        board.ResetBoard();

        menuPanel.SetActive(false);
        confirmationPanel.SetActive(false);
        _isGameActive = true;

        _score = 0;
        UpdateScoreUI();

        Rect worldBounds = GetWorldBounds(boardPanelRect);
        board.Initialize(worldBounds);
        snake.Initialize();
        board.SpawnFood();
    }

    private void HandleFoodEaten()
    {
        if (!_isGameActive) return;

        if (biteSoundClip != null)
        {
            _audioSource.PlayOneShot(biteSoundClip);
        }

        _score++;
        UpdateScoreUI();
        board.SpawnFood();
    }

    private void HandleGameOver()
    {
        if (!_isGameActive) return;

        _isGameActive = false;

        if (_score > _highScore)
        {
            _highScore = _score;
            SaveHighScore();
        }

        ShowMenu(false);
    }

    private void ShowMenu(bool isFirstLaunch)
    {
        menuPanel.SetActive(true);
        confirmationPanel.SetActive(false);

        if (isFirstLaunch)
        {
            menuMessage.text = "SNAKE GAME";
        }
        else
        {
            menuMessage.text = "GAME OVER";
        }

        LoadHighScore();
        UpdateScoreUI();
    }

    private void LoadHighScore()
    {
        _highScore = PlayerPrefs.GetInt("HighScore", 0);
    }

    private void SaveHighScore()
    {
        PlayerPrefs.SetInt("HighScore", _highScore);
    }

    private void UpdateScoreUI()
    {
        if (scoreText != null)
        {
            scoreText.text = _score.ToString();
        }
        if (highScoreText != null)
        {
            highScoreText.text = _highScore.ToString();
        }
    }

    public void OnResetHighScoreButton()
    {
        confirmationPanel.SetActive(true);
        menuPanel.SetActive(false);
    }

    public void OnConfirmResetButton()
    {
        _highScore = 0;
        PlayerPrefs.SetInt("HighScore", 0);
        PlayerPrefs.Save();
        UpdateScoreUI();

        confirmationPanel.SetActive(false);
        menuPanel.SetActive(true);
    }

    public void OnCancelResetButton()
    {
        menuPanel.SetActive(true);
        confirmationPanel.SetActive(false);
    }

    private Rect GetWorldBounds(RectTransform rectTransform)
    {
        Vector3[] corners = new Vector3[4];
        rectTransform.GetWorldCorners(corners);

        float xMin = Mathf.Round(corners[0].x);
        float yMin = Mathf.Round(corners[0].y);
        float xMax = Mathf.Round(corners[2].x);
        float yMax = Mathf.Round(corners[2].y);

        return new Rect(xMin, yMin, xMax - xMin, yMax - yMin);
    }

    private void OnDestroy()
    {
        if (snake != null)
        {
            snake.OnAteFood -= HandleFoodEaten;
            snake.OnDied -= HandleGameOver;
        }
    }
}