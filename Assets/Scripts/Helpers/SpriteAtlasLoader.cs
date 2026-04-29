using UnityEngine;
using UnityEngine.U2D;

/// <summary>Loads player cosmetic sprites (backgrounds, obstacles) from a sprite atlas based on PlayerPrefs.</summary>
public sealed class SpriteAtlasLoader : MonoBehaviour
{
    [SerializeField] SpriteAtlas spriteAtlas;
    enum SpriteType { Background, Obstacle }
    [SerializeField] SpriteType type;
    [SerializeField] SpriteRenderer sprite;

    // Load the correct cosmetic sprite based on player's saved selection
    void Awake()
    {
        if (type == SpriteType.Background)
            sprite.sprite = spriteAtlas.GetSprite("Background" + PlayerPrefs.GetInt("BackgroundSelected", 0));
        else
            sprite.sprite = spriteAtlas.GetSprite("Obstacle" + PlayerPrefs.GetInt("ObstacleSelected", 0));
    }
}