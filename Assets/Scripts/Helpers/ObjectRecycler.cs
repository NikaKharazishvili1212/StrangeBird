using UnityEngine;

// Recycles objects that leave the screen by deactivating them for object pooling
public sealed class ObjectRecycler : MonoBehaviour
{
    readonly string[] recyclableTags = { "Coin", "Obstacle", "Bird" };

    void OnTriggerEnter2D(Collider2D other)
    {
        foreach (string tag in recyclableTags)
        {
            if (other.CompareTag(tag))
            {
                other.gameObject.SetActive(false);
                return;
            }
        }

        // Ignore "Enemy" tagged objects (child obstacles that shouldn't be recycled individually)
        if (!other.CompareTag("Enemy")) Debug.LogWarning($"Unknown object '{other.name}' touched Object Recycler!");
    }
}