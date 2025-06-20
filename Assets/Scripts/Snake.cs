using System;
using System.Collections.Generic;
using UnityEngine;

public class Snake : MonoBehaviour
{
    public event Action OnAteFood;
    public event Action OnDied;

    [Header("Settings")]
    [SerializeField] private SnakeSettings settings;
    [SerializeField] private Transform segmentPrefab;


    [Header("References")]
    [SerializeField] private Board board;

    private List<Vector2Int> _segments;
    private List<Transform> _segmentObjects;
    private Vector2Int _direction = Vector2Int.right;
    private Vector2Int _inputDirection;
    private bool _shouldGrow = false;

    private void Update()
    {
        HandleInput();
    }

    public void Initialize()
    {
        _segments = new List<Vector2Int>();
        _segmentObjects = new List<Transform>();
        _inputDirection = Vector2Int.right;

        int startX = Mathf.FloorToInt(board.WorldBounds.xMin) + 5;
        int startY = Mathf.FloorToInt(board.WorldBounds.center.y);

        for (int i = 0; i < settings.initialSize; i++)
        {
            _segments.Add(new Vector2Int(startX - i, startY));
        }

        foreach (var pos in _segments)
        {
            _segmentObjects.Add(Instantiate(segmentPrefab, (Vector2)pos, Quaternion.identity));
        }

        board.UpdateSnakeOnGrid(new List<Vector2Int>(), _segments);

        InvokeRepeating(nameof(GameTick), 0f, settings.moveSpeed);
    }

    private void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
        {
            if (_direction != Vector2Int.down) _inputDirection = Vector2Int.up;
        }
        else if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
        {
            if (_direction != Vector2Int.up) _inputDirection = Vector2Int.down;
        }
        else if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
        {
            if (_direction != Vector2Int.right) _inputDirection = Vector2Int.left;
        }
        else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
        {
            if (_direction != Vector2Int.left) _inputDirection = Vector2Int.right;
        }
    }

    private void GameTick()
    {
        _direction = _inputDirection;
        Vector2Int newHeadPosition = _segments[0] + _direction;

        GridEntityType whatsNext = board.GetEntityTypeAt(newHeadPosition);

        switch (whatsNext)
        {
            case GridEntityType.Wall:
                Die();
                return;

            case GridEntityType.Snake:
                Vector2Int tailPosition = _segments[_segments.Count - 1];
                if (newHeadPosition == tailPosition && !_shouldGrow)
                {
                    break;
                }
                else
                {
                    Die();
                    return;
                }

            case GridEntityType.Food:
                _shouldGrow = true;
                OnAteFood?.Invoke();
                break;

            case GridEntityType.Empty:
                break;
        }

        Move(newHeadPosition);
    }

    private void Move(Vector2Int newHeadPosition)
    {
        List<Vector2Int> oldPositions = new List<Vector2Int>(_segments);

        _segments.Insert(0, newHeadPosition);

        if (_shouldGrow)
        {
            var obj = Instantiate(segmentPrefab, (Vector2)_segments[0], Quaternion.identity);
            _segmentObjects.Insert(0, obj);
            _shouldGrow = false;
        }
        else
        {
            _segments.RemoveAt(_segments.Count - 1);
        }

        board.UpdateSnakeOnGrid(oldPositions, _segments);
        SyncVisuals();
    }

    private void Die()
    {
        OnDied?.Invoke();
        CancelInvoke(nameof(GameTick));
    }

    private void SyncVisuals()
    {
        for (int i = 0; i < _segments.Count; i++)
        {
            _segmentObjects[i].position = (Vector2)_segments[i];
        }
    }
}