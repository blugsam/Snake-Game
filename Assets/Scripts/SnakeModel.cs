using System;
using System.Collections.Generic;
using UnityEngine;

public class SnakeModel
{
    public IReadOnlyList<Vector2Int> Segments => _segments;
    private readonly List<Vector2Int> _segments = new List<Vector2Int>();

    public Vector2Int Direction { get; private set; }
    public bool IsDead { get; private set; }

    private readonly int _initialSize;
    private readonly Rect _worldBounds;

    public SnakeModel(int initialSize, Rect worldBounds)
    {
        _initialSize = initialSize;
        _worldBounds = worldBounds;
        Reset();
    }

    public void Reset()
    {
        IsDead = false;
        Direction = Vector2Int.right;

        _segments.Clear();
        int startX = Mathf.FloorToInt(_worldBounds.xMin) + 5;
        int startY = Mathf.FloorToInt(_worldBounds.center.y);
        for (int i = 0; i < _initialSize; i++)
        {
            _segments.Add(new Vector2Int(startX - i, startY));
        }
    }

    public void SetDirection(Vector2Int newDirection)
    {
        if (newDirection != -Direction)
        {
            Direction = newDirection;
        }
    }

    public Vector2Int GetNextHeadPosition()
    {
        return _segments[0] + Direction;
    }

    public void Move(Vector2Int newHead)
    {
        _segments.Insert(0, newHead);
        _segments.RemoveAt(_segments.Count - 1);
    }

    public void Grow(Vector2Int newHead)
    {
        _segments.Insert(0, newHead);
    }

    public void Die()
    {
        IsDead = true;
    }
}