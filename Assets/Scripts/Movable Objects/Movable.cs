using UnityEngine;

/// <summary>Base class for all movable game objects (coins, obstacles, birds).</summary>
[System.Serializable]
public abstract class Movable : MonoBehaviour
{
    [SerializeField] protected GameManager gameManager;
    [SerializeField] protected Rigidbody2D rb;

    public abstract void TeleportToStartingPosition();

    public abstract void Move();
    
    public abstract void Stop();
}