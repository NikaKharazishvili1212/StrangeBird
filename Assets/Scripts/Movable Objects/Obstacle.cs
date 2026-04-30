using UnityEngine;
using static Utils;
using static Constants;

/// <summary>Handles obstacle movement, horizontal scrolling, and vertical ping-pong based on difficulty.</summary>
public sealed class Obstacle : Movable
{
    static float moveSpeed;
    static float basePingPongSpeed;
    float pingPongSpeed;

    // Called by GameManager once at game start; sets move and ping-pong speed based on difficulty
    public static void SetSpeed(int difficulty)
    {
        moveSpeed = difficulty == 0 ? EasyObstacleSpeed : difficulty == 1 ? MediumObstacleSpeed : HardObstacleSpeed;
        basePingPongSpeed = difficulty == 0 ? EasyObstaclePingPongSpeed : difficulty == 1 ? MediumObstaclePingPongSpeed : HardObstaclePingPongSpeed;
    }

    // Initialize obstacle on spawn: randomize ping-pong direction, reposition and start moving
    void OnEnable()
    {
        pingPongSpeed = PercentChance(50) ? basePingPongSpeed : -basePingPongSpeed;
        TeleportToStartingPosition();
        Move();
    }

    // Reverse vertical direction at boundaries and apply velocity each physics tick
    void FixedUpdate()
    {
        if (rb.linearVelocity.x == 0) return;
        
        bool hitCeiling = transform.position.y >= ObstaclePingPongYMax && pingPongSpeed > 0;
        bool hitFloor = transform.position.y <= ObstaclePingPongYMin && pingPongSpeed < 0;
        if (hitCeiling || hitFloor) pingPongSpeed = -pingPongSpeed;
        rb.linearVelocity = new Vector2(moveSpeed, pingPongSpeed);
    }

    // Teleport to the right side of the screen with random Y position (for pooling)
    public override void TeleportToStartingPosition() => transform.position = new Vector2(ObstacleSpawnX, Random.Range(-SpawnY, SpawnY));

    // Move left with ping-pong
    public override void Move() => rb.linearVelocity = new Vector2(moveSpeed, pingPongSpeed);

    // Stops obstacle movement when a player dies. Called by game manager
    public override void Stop() => rb.linearVelocity = new Vector2(0, 0);
}