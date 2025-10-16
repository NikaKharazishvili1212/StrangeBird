using UnityEngine;

[System.Serializable]
// Base class for all movable game objects (coins, obstacles, birds)
public abstract class Movable : MonoBehaviour
{
    [SerializeField] protected GameManager gameManager;
    [SerializeField] protected Rigidbody2D rb;
    public enum MoveDirection { None = 0, Left = 1, Right = 2 };

    public abstract void TeleportToRight();
    public abstract void Move(MoveDirection direction);
}