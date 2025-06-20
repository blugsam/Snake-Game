using UnityEngine;

[CreateAssetMenu(fileName = "SnakeSettings", menuName = "Game/Snake Settings")]
public class SnakeSettings : ScriptableObject
{
    public float moveSpeed = 0.2f;
    public int initialSize = 4;
    public Color snakeColor = Color.green;
}