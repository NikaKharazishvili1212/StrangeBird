using UnityEngine;
using static Constants;

/// <summary>Handles coin movement and speed scaling based on game speed.</summary>
public sealed class Coin : Movable
{
    static float moveSpeed;
    
    // Called by GameManager once at game start
    public static void SetSpeed(int gameSpeed) => moveSpeed = gameSpeed == 0 ? SlowCoinSpeed : gameSpeed == 1 ? MediumCoinSpeed : FastCoinSpeed;

    void Update() => DeactivateOnLeavingScreen();

    // Initialize coin on spawn: reposition and start moving
    void OnEnable()
    {
        TeleportToStartingPosition();
        Move();
    }

    // Teleport to the right side of the screen with random Y position (for pooling)
    public override void TeleportToStartingPosition() => transform.position = new Vector2(CoinSpawnX, Random.Range(-CoinSpawnY, CoinSpawnY));

    // Coin only moves left
    public override void Move() => rb.linearVelocity = new Vector2(moveSpeed, 0);

    // Stops coin movement when a player dies. Called by game manager
    public override void Stop() => rb.linearVelocity = new Vector2(0, 0);
}