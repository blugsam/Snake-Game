using System.Collections.Generic;
using UnityEngine;

public class SnakeSegmentPool
{
    private readonly Queue<Transform> _pool = new();
    private readonly SnakeSegmentFactory _factory;

    public SnakeSegmentPool(SnakeSegmentFactory factory)
    {
        _factory = factory;
    }

    public Transform Get()
    {
        if (_pool.Count > 0)
        {
            var seg = _pool.Dequeue();
            seg.gameObject.SetActive(true);
            return seg;
        }

        return _factory.Create();
    }

    public void Return(Transform segment)
    {
        segment.gameObject.SetActive(false);
        _pool.Enqueue(segment);
    }

    public void Clear()
    {
        _pool.Clear();
    }

    public int Count => _pool.Count;
}