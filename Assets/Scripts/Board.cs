using System.Collections.Generic;
using UnityEngine;

public enum GridEntityType
{
    Empty,
    Snake,
    Food,
    Wall
}
public class Board : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject foodPrefab;
    public Rect WorldBounds { get; private set; }

    private Dictionary<Vector2Int, GridEntityType> _grid = new Dictionary<Vector2Int, GridEntityType>();
    private GameObject _foodObject;
    public Vector2Int FoodPosition { get; private set; }

    public void Initialize(Rect worldBounds)
    {
        _grid.Clear();
        this.WorldBounds = worldBounds;
    }

    public GridEntityType GetEntityTypeAt(Vector2Int position)
    {
        if (position.x <= WorldBounds.xMin || position.x >= WorldBounds.xMax ||
            position.y <= WorldBounds.yMin || position.y >= WorldBounds.yMax)
        {
            return GridEntityType.Wall;
        }

        if (_grid.TryGetValue(position, out GridEntityType type))
        {
            return type;
        }

        return GridEntityType.Empty;
    }

    public void UpdateSnakeOnGrid(List<Vector2Int> oldPositions, List<Vector2Int> newPositions)
    {
        foreach (var pos in oldPositions)
        {
            _grid.Remove(pos);
        }

        if (newPositions.Count > 0)
        {
            _grid[newPositions[0]] = GridEntityType.Snake;
            for (int i = 1; i < newPositions.Count; i++)
            {
                _grid[newPositions[i]] = GridEntityType.Snake;
            }
        }
    }

    public void SpawnFood()
    {
        if (_foodObject != null)
        {
            Destroy(_foodObject);
        }

        Vector2Int newFoodPosition;
        do
        {
            newFoodPosition = new Vector2Int(
                Random.Range((int)WorldBounds.xMin + 1, (int)WorldBounds.xMax),
                Random.Range((int)WorldBounds.yMin + 1, (int)WorldBounds.yMax)
            );
        } while (GetEntityTypeAt(newFoodPosition) != GridEntityType.Empty);

        FoodPosition = newFoodPosition;
        _grid[FoodPosition] = GridEntityType.Food;
        _foodObject = Instantiate(foodPrefab, (Vector2)FoodPosition, Quaternion.identity);
    }

    public void UpdateCell(Vector2Int pos, GridEntityType newType)
    {
        if (newType == GridEntityType.Empty)
            _grid.Remove(pos);
        else
            _grid[pos] = newType;
    }

    public void ResetBoard()
    {
        _grid.Clear();

        if (_foodObject != null)
        {
            Destroy(_foodObject);
            _foodObject = null;
        }
    }
}