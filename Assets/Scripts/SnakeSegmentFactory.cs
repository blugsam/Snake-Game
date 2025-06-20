using UnityEngine;

public class SnakeSegmentFactory
{
    private readonly Transform _segmentPrefab;

    public SnakeSegmentFactory(Transform segmentPrefab)
    {
        _segmentPrefab = segmentPrefab;
    }

    public Transform Create()
    {
        return Object.Instantiate(_segmentPrefab);
    }
}