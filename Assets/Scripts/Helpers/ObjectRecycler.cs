using UnityEngine;

/// <summary>Recycles objects that leave the screen by deactivating them for object pooling.</summary>
public sealed class ObjectRecycler : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Coin") || other.CompareTag("Obstacle") || other.CompareTag("Bird"))
            other.gameObject.SetActive(false);
    }
}