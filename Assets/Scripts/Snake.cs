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
    [SerializeField] private Transform eyesPrefab;

    [Header("References")]
    [SerializeField] private Board board;

    //logic
    private List<Vector2Int> _segments;
    private List<Vector2Int> _previousSegments;

    //visuals
    private List<Transform> _segmentObjects;
    private Transform _eyes;
    private SnakeSegmentFactory _factory;
    private SnakeSegmentPool _segmentPool;

    //movement handle
    private Vector2Int _direction = Vector2Int.right;
    private Vector2Int _inputDirection;
    private float _moveTimer;
    private bool _shouldGrow = false;
    private bool _isDead = false;

    private void Update()
    {
        if (_isDead) return;

        HandleInput();
        UpdateMovement();
    }

    public void Initialize()
    {
        _factory ??= new SnakeSegmentFactory(segmentPrefab);
        _segmentPool ??= new SnakeSegmentPool(_factory);

        _isDead = false;
        _moveTimer = 0f;
        _shouldGrow = false;
        _direction = Vector2Int.right;
        _inputDirection = Vector2Int.right;

        _segments = new List<Vector2Int>();
        _segmentObjects = new List<Transform>();

        int startX = Mathf.FloorToInt(board.WorldBounds.xMin) + 5;
        int startY = Mathf.FloorToInt(board.WorldBounds.center.y);

        for (int i = 0; i < settings.initialSize; i++)
        {
            _segments.Add(new Vector2Int(startX - i, startY));
        }

        _previousSegments = new List<Vector2Int>(_segments);

        foreach (var pos in _segments)
        {
            var segObj = _segmentPool.Get();
            segObj.position = (Vector2)pos;
            _segmentObjects.Add(segObj);
            board.UpdateCell(pos, GridEntityType.Snake);
        }

        if (eyesPrefab != null)
        {
            _eyes = Instantiate(eyesPrefab);
            UpdateEyes();
        }
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

    private void UpdateMovement()
    {
        _moveTimer += Time.deltaTime;

        if (_moveTimer >= settings.moveSpeed)
        {
            _moveTimer -= settings.moveSpeed;
            GameTick();
        }

        SyncVisualsSmoothly();
    }

    private void GameTick()
    {
        _previousSegments = new List<Vector2Int>(_segments);

        bool directionChanged = _direction != _inputDirection;
        _direction = _inputDirection;

        if (directionChanged)
        {
            UpdateEyes();
        }

        Vector2Int newHeadPosition = _segments[0] + _direction;

        GridEntityType whatsNext = board.GetEntityTypeAt(newHeadPosition);
        if (whatsNext == GridEntityType.Wall ||
           (whatsNext == GridEntityType.Snake && !(newHeadPosition == _segments[^1])))
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
        _segments.Insert(0, newHead);
        board.UpdateCell(newHead, GridEntityType.Snake);

        if (_shouldGrow)
        {
            var newSegObj = _segmentPool.Get();
            _segmentObjects.Insert(0, newSegObj);
            _shouldGrow = false;
        }
        else
        {
            Vector2Int oldTail = _segments[^1];
            _segments.RemoveAt(_segments.Count - 1);
            board.UpdateCell(oldTail, GridEntityType.Empty);
        }

        UpdateEyes();
    }

    private void UpdateEyes()
    {
        if (_eyes == null || _segmentObjects.Count == 0) return;

        _eyes.SetParent(_segmentObjects[0], false);
        _eyes.localPosition = Vector3.zero;

        _eyes.rotation = Quaternion.Euler(0, 0, _direction switch
        {
            Vector2Int d when d == Vector2Int.up => 90,
            Vector2Int d when d == Vector2Int.down => -90,
            Vector2Int d when d == Vector2Int.left => 180,
            _ => 0,
        });
    }

    private void SyncVisualsSmoothly()
    {
        float t = _moveTimer / settings.moveSpeed;

        for (int i = 0; i < _segmentObjects.Count; i++)
        {
            Vector2 startPos, endPos;

            if (i < _previousSegments.Count)
            {
                startPos = (Vector2)_previousSegments[i];
                endPos = (Vector2)_segments[i];
            }
            else
            {
                startPos = (Vector2)_segments[i];
                endPos = (Vector2)_segments[i];
            }

            _segmentObjects[i].position = Vector2.Lerp(startPos, endPos, t);
        }
    }

    private void Die()
    {
        if (_isDead) return;
        _isDead = true;
        OnDied?.Invoke();
    }

    public void ResetSnake()
    {
        foreach (var seg in _segmentObjects)
            _segmentPool.Return(seg);

        _segmentObjects.Clear();
        _segments?.Clear();
        _previousSegments?.Clear();

        if (_eyes != null)
        {
            Destroy(_eyes.gameObject);
            _eyes = null;
        }

        _isDead = true;
    }
}