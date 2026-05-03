using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UI;

/// <summary>Loads player cosmetic sprites (backgrounds, obstacles) from a sprite atlas based on PlayerPrefs.</summary>
public sealed class SpriteAtlasLoader : MonoBehaviour
{
    [SerializeField] SpriteAtlas spriteAtlas;
    [SerializeField] Image image; // Background, if not null
    [SerializeField] SpriteRenderer sprite; // Obstacle, if not null

    // Load the correct cosmetic sprite based on player's saved selection
    void Awake()
    {
        if (image) image.sprite = spriteAtlas.GetSprite("Background" + PlayerPrefs.GetInt("BackgroundSelected", 0));
        else sprite.sprite = spriteAtlas.GetSprite("Obstacle" + PlayerPrefs.GetInt("ObstacleSelected", 0));
    }
}