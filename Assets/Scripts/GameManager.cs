using TMPro;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class GameManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Board board;
    [SerializeField] private SnakeController snakeController;
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

    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();

        snakeController.OnAteFood += HandleFoodEaten;
        snakeController.OnDied += HandleGameOver;
    }

    private void Start()
    {
        confirmationPanel.SetActive(false);
        ShowMenu(true);
    }

    private void StartGame()
    {
        menuPanel.SetActive(false);
        confirmationPanel.SetActive(false);

        board.ResetBoard();
        Rect worldBounds = GetWorldBounds(boardPanelRect);
        board.Initialize(worldBounds);

        _score = 0;
        UpdateScoreUI();

        snakeController.StartNewGame();
        board.SpawnFood();
    }

    private void StopGame()
    {
        snakeController.StopGame();

        if (_score > _highScore)
        {
            _highScore = _score;
            SaveHighScore();
        }

        ShowMenu(false);
    }

    public void OnPlayButton()
    {
        StartGame();
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

        menuPanel.SetActive(true);
        confirmationPanel.SetActive(false);
    }

    public void OnCancelResetButton()
    {
        menuPanel.SetActive(true);
        confirmationPanel.SetActive(false);
    }

    private void HandleFoodEaten()
    {
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
        StopGame();
    }

    private void ShowMenu(bool isFirstLaunch)
    {
        menuPanel.SetActive(true);

        if (isFirstLaunch)
        {
            menuMessage.text = "SNAKE";
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
        scoreText.text = _score.ToString();
        highScoreText.text = _highScore.ToString();
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
        if (snakeController != null)
        {
            snakeController.OnAteFood -= HandleFoodEaten;
            snakeController.OnDied -= HandleGameOver;
        }
    }
}