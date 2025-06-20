using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SnakeController : MonoBehaviour
{
    public event Action OnAteFood;
    public event Action OnDied;

    [Header("Settings")]
    [SerializeField] private SnakeSettings settings;

    [Header("References")]
    [SerializeField] private Board board;
    [SerializeField] private SnakeView view;

    private SnakeModel _model;
    private List<Vector2Int> _previousSegments;

    private const int MaxQueuedInputs = 2;
    private readonly Queue<Vector2Int> _inputQueue = new Queue<Vector2Int>();
    private float _moveTimer;

    private bool _isGameActive = false;

    private void Update()
    {
        if (!_isGameActive || _model.IsDead) return;

        HandleInput();
        UpdateMovement();

        view.Render(_model.Segments, _previousSegments, _model.Direction, _moveTimer / settings.moveSpeed);
    }

    public void StartNewGame()
    {
        _model = new SnakeModel(settings.initialSize, board.WorldBounds);
        _previousSegments = new List<Vector2Int>(_model.Segments);
        _inputQueue.Clear();
        _moveTimer = 0f;
        _isGameActive = true;

        foreach (var pos in _model.Segments) board.UpdateCell(pos, GridEntityType.Snake);
    }

    public void StopGame()
    {
        _isGameActive = false;
        view.Clear();
    }

    private void HandleInput()
    {
        Vector2Int candidate = Vector2Int.zero;
        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow)) candidate = Vector2Int.up;
        else if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow)) candidate = Vector2Int.down;
        else if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow)) candidate = Vector2Int.left;
        else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow)) candidate = Vector2Int.right;
        else return;

        Vector2Int last = _inputQueue.Count > 0 ? _inputQueue.Last() : _model.Direction;
        if (candidate == last || candidate + last == Vector2Int.zero) return;
        if (_inputQueue.Count < MaxQueuedInputs) _inputQueue.Enqueue(candidate);
    }

    private void UpdateMovement()
    {
        _moveTimer += Time.deltaTime;
        if (_moveTimer >= settings.moveSpeed)
        {
            _moveTimer -= settings.moveSpeed;
            GameTick();
        }
    }

    private void GameTick()
    {
        _previousSegments = new List<Vector2Int>(_model.Segments);

        if (_inputQueue.Count > 0)
        {
            _model.SetDirection(_inputQueue.Dequeue());
        }

        Vector2Int newHeadPosition = _model.GetNextHeadPosition();
        GridEntityType whatsNext = board.GetEntityTypeAt(newHeadPosition);

        switch (whatsNext)
        {
            case GridEntityType.Wall:
            case GridEntityType.Snake:
                _model.Die();
                OnDied?.Invoke();
                _isGameActive = false;
                break;

            case GridEntityType.Food:
                Vector2Int oldTailBeforeGrow = _model.Segments[^1];
                _model.Grow(newHeadPosition);
                board.UpdateCell(newHeadPosition, GridEntityType.Snake);
                OnAteFood?.Invoke();
                break;

            case GridEntityType.Empty:
                Vector2Int oldTail = _model.Segments[^1];
                _model.Move(newHeadPosition);
                board.UpdateCell(oldTail, GridEntityType.Empty);
                board.UpdateCell(newHeadPosition, GridEntityType.Snake);
                break;
        }
    }
}