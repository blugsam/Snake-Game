using System.Collections.Generic;
using UnityEngine;

public class SnakeView : MonoBehaviour
{
    [SerializeField] private Transform segmentPrefab;
    [SerializeField] private Transform eyesPrefab;

    private readonly List<Transform> _segmentObjects = new List<Transform>();
    private Transform _eyes;

    private SnakeSegmentFactory _factory;
    private SnakeSegmentPool _pool;

    private void Awake()
    {
        _factory = new SnakeSegmentFactory(segmentPrefab);
        _pool = new SnakeSegmentPool(_factory);
    }

    public void Render(IReadOnlyList<Vector2Int> segments, IReadOnlyList<Vector2Int> prevSegments, Vector2Int direction, float interpolation)
    {
        while (_segmentObjects.Count < segments.Count)
        {
            _segmentObjects.Insert(0, _pool.Get());
        }
        while (_segmentObjects.Count > segments.Count)
        {
            _pool.Return(_segmentObjects[^1]);
            _segmentObjects.RemoveAt(_segmentObjects.Count - 1);
        }

        for (int i = 0; i < _segmentObjects.Count; i++)
        {
            Vector2 startPos = (i < prevSegments.Count) ? (Vector2)prevSegments[i] : (Vector2)segments[i];
            Vector2 endPos = (Vector2)segments[i];
            _segmentObjects[i].position = Vector2.Lerp(startPos, endPos, interpolation);
        }

        UpdateEyes(direction);
    }

    private void UpdateEyes(Vector2Int direction)
    {
        if (_eyes == null) _eyes = Instantiate(eyesPrefab);
        if (_segmentObjects.Count == 0) return;

        _eyes.SetParent(_segmentObjects[0], false);
        _eyes.localPosition = Vector3.zero;
        _eyes.rotation = Quaternion.Euler(0, 0, direction switch
        {
            Vector2Int d when d == Vector2Int.up => 90,
            Vector2Int d when d == Vector2Int.down => -90,
            Vector2Int d when d == Vector2Int.left => 180,
            _ => 0
        });
    }

    public void Clear()
    {
        foreach (var segment in _segmentObjects)
        {
            _pool.Return(segment);
        }
        _segmentObjects.Clear();

        if (_eyes != null)
        {
            Destroy(_eyes.gameObject);
            _eyes = null;
        }
    }
}