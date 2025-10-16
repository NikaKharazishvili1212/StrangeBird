using UnityEngine;

using static Constants;

public sealed class Coin : Movable
{
    static float moveSpeed;
    
    // Called by GameManager once at game start; Coin's move speed depends on difficulty
    public static void SetSpeed(int difficulty) => moveSpeed = difficulty == 0 ? EasyCoinSpeed : difficulty == 1 ? MediumCoinSpeed : HardCoinSpeed;

    void OnEnable()
    {
        TeleportToRight();
        Move(MoveDirection.Left);
    }

    // Teleport to the right side of the screen with random Y position (for pooling)
    public override void TeleportToRight() => transform.position = new Vector2(8f, Random.Range(-2f, 2f));

    // Coin only moves left (or stops if player dies)
    public override void Move(MoveDirection direction)
    {
        if (direction == MoveDirection.Left) rb.velocity = new Vector2(moveSpeed, 0f);
        else if (direction == MoveDirection.None) rb.velocity = Vector2.zero;
        else Debug.LogWarning("Invalid direction! Coin can only move left or stop moving!");
    }
}