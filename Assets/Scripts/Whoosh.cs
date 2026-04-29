using UnityEngine;

/// <summary>Pooled by player on each jump; has it's own lifespan.</summary>
public class Whoosh : MonoBehaviour
{
    float lifespan = Constants.WhooshLifespan;

    // Called by player
    public void Activate()
    {
        lifespan = Constants.WhooshLifespan;
        gameObject.SetActive(true);
    }

    // Deactivate after lifespan
    void Update()
    {
        if (lifespan <= 0) gameObject.SetActive(false);
        lifespan -= Time.deltaTime;
    }
}