using UnityEngine;

/// <summary>Base class for all movable game objects (coins, obstacles, birds).</summary>
[System.Serializable]
public abstract class Movable : MonoBehaviour
{
    [SerializeField] protected Rigidbody2D rb;

    // Called by GameManager to pool this object
    public void Activate() => gameObject.SetActive(true);

    // Deactivates on leaving screen to be pooled again later. Overriden only by Bird class
    public virtual void DeactivateOnLeavingScreen() { if (transform.position.x <= -6) gameObject.SetActive(false); }

    public abstract void TeleportToStartingPosition();

    public abstract void Move();

    public abstract void Stop();
}