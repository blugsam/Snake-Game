using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Board board;
    [SerializeField] private Snake snake;
    [SerializeField] private RectTransform boardPanelRect;
    [Header("UI")]
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text highScoreText;
    [SerializeField] private GameObject gameOverPanel;

    private int _score;
    private int _highScore;

    private void Awake()
    {
        Rect worldBounds = GetWorldBounds(boardPanelRect);

        board.Initialize(worldBounds);

        snake.Initialize();

        snake.OnAteFood += HandleFoodEaten;
        snake.OnDied += HandleGameOver;
    }

    private void Start()
    {
        gameOverPanel.SetActive(false);
        LoadHighScore();
        UpdateScoreUI();
        board.SpawnFood();
    }

    private void HandleFoodEaten()
    {
        _score++;
        UpdateScoreUI();
        board.SpawnFood();
    }

    private void HandleGameOver()
    {
        if (_score > _highScore)
        {
            _highScore = _score;
            SaveHighScore();
        }
        gameOverPanel.SetActive(true);
    }

    public void RestartGame()
    {
        gameOverPanel.SetActive(false);
        _score = 0;
        UpdateScoreUI();

        board.ResetBoard();
        snake.ResetSnake();
        snake.Initialize();
        board.SpawnFood();
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