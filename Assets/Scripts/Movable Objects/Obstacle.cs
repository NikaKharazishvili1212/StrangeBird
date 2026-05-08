using UnityEngine;
using static Utils;
using static Constants;

/// <summary>Handles obstacle movement, horizontal scrolling, and vertical ping-pong based on game speed.</summary>
public sealed class Obstacle : Movable
{
    [SerializeField] SpriteRenderer spriteRenderer1, spriteRenderer2;
    static float moveSpeed;
    static float basePingPongSpeed;
    float pingPongSpeed;

    // Called by GameManager once at game start; sets move and ping-pong speed based on game speed
    public static void SetSpeed(int gameSpeed)
    {
        moveSpeed = gameSpeed == 0 ? SlowObstacleSpeed : gameSpeed == 1 ? MediumObstacleSpeed : FastObstacleSpeed;
        basePingPongSpeed = gameSpeed == 0 ? SlowObstaclePingPongSpeed : gameSpeed == 1 ? MediumObstaclePingPongSpeed : FastObstaclePingPongSpeed;
    }

    // Load obstacle models having and selected by player
    void Awake()
    {
        spriteRenderer1.sprite = GameManager.Instance.GetSpriteAtlas().GetSprite("Obstacle" + PlayerPrefs.GetInt("ObstacleSelected", 0));
        spriteRenderer2.sprite = GameManager.Instance.GetSpriteAtlas().GetSprite("Obstacle" + PlayerPrefs.GetInt("ObstacleSelected", 0));
    }

    void Update() => DeactivateOnLeavingScreen();

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

        bool hitCeiling = transform.position.y >= ObstacleSpawnY && pingPongSpeed > 0;
        bool hitFloor = transform.position.y <= -ObstacleSpawnY && pingPongSpeed < 0;
        if (hitCeiling || hitFloor) pingPongSpeed = -pingPongSpeed;
        rb.linearVelocity = new Vector2(moveSpeed, pingPongSpeed);
    }

    // Teleport to the right side of the screen with random Y position (for pooling)
    public override void TeleportToStartingPosition() => transform.position = new Vector2(ObstacleSpawnX, Random.Range(-ObstacleSpawnY, ObstacleSpawnY));

    // Move left with ping-pong
    public override void Move() => rb.linearVelocity = new Vector2(moveSpeed, pingPongSpeed);

    // Stops obstacle movement when a player dies. Called by game manager
    public override void Stop() => rb.linearVelocity = new Vector2(0, 0);
}