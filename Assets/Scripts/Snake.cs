using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;

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
    private SnakeSegmentFactory _factory;
    private SnakeSegmentPool _segmentPool;

    private void Update()
    {
        HandleInput();
    }

    public void Initialize()
    {
        _factory ??= new SnakeSegmentFactory(segmentPrefab);
        _segmentPool ??= new SnakeSegmentPool(_factory);

        _segments = new List<Vector2Int>();
        _segmentObjects = new List<Transform>();
        _inputDirection = Vector2Int.right;

        int startX = Mathf.FloorToInt(board.WorldBounds.xMin) + 5;
        int startY = Mathf.FloorToInt(board.WorldBounds.center.y);

        for (int i = 0; i < settings.initialSize; i++)
            _segments.Add(new Vector2Int(startX - i, startY));

        foreach (var pos in _segments)
        {
            var segObj = _segmentPool.Get();
            segObj.position = (Vector2)pos;
            _segmentObjects.Add(segObj);
            board.UpdateCell(pos, GridEntityType.Snake);
        }

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
        if (whatsNext == GridEntityType.Wall ||
           (whatsNext == GridEntityType.Snake &&
             !(newHeadPosition == _segments[^1] && !_shouldGrow)))
        {
            Die();
            return;
        }

        if (whatsNext == GridEntityType.Food)
        {
            _shouldGrow = true;
            OnAteFood?.Invoke();
        }

        Move(newHeadPosition);
    }

    private void Move(Vector2Int newHead)
    {
        Vector2Int? oldTail = _shouldGrow
            ? (Vector2Int?)null
            : _segments[^1];

        _segments.Insert(0, newHead);

        if (_shouldGrow)
        {
            var newSeg = _segmentPool.Get();
            newSeg.position = (Vector2)newHead;
            _segmentObjects.Insert(0, newSeg);
            _shouldGrow = false;
        }
        else
        {
            var tailObj = _segmentObjects[^1];
            _segmentObjects.RemoveAt(_segmentObjects.Count - 1);

            tailObj.position = (Vector2)newHead;
            _segmentObjects.Insert(0, tailObj);

            _segments.RemoveAt(_segments.Count - 1);
        }

        if (oldTail.HasValue)
            board.UpdateCell(oldTail.Value, GridEntityType.Empty);

        board.UpdateCell(newHead, GridEntityType.Snake);

        SyncVisuals();
        ValidateState();
    }

    private void Die()
    {
        CancelInvoke(nameof(GameTick));

        Debug.Log($"Die: Returning {_segmentObjects.Count} segments to pool");

        foreach (var seg in _segmentObjects)
            _segmentPool.Return(seg);
        _segmentObjects.Clear();
        _segments.Clear();

        OnDied?.Invoke();
    }

    private void SyncVisuals()
    {
        for (int i = 0; i < _segments.Count; i++)
            _segmentObjects[i].position = (Vector2)_segments[i];
    }

    public void ResetSnake()
    {
        CancelInvoke(nameof(GameTick));

        foreach (var seg in _segmentObjects)
            _segmentPool.Return(seg);

        _segmentObjects.Clear();
        _segments.Clear();
        _shouldGrow = false;
        _direction = Vector2Int.right;
        _inputDirection = Vector2Int.right;
    }

    [Conditional("UNITY_EDITOR")]
    private void ValidateState()
    {
        //количество логических позиций и визуальных объектов должно совпадать
        Debug.Assert(_segments.Count == _segmentObjects.Count,
            $"Mismatch: segments({_segments.Count}) vs objects({_segmentObjects.Count})");

        //все объекты должны быть активны и синхронизированы
        for (int i = 0; i < _segments.Count; i++)
        {
            Vector2Int pos = _segments[i];
            Vector3 worldPos = _segmentObjects[i].position;
            Debug.Assert((Vector2)worldPos == (Vector2)pos,
                $"Segment #{i} at {worldPos} but _segments has {pos}");
        }
    }
}
