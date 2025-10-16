using UnityEngine;

// Partial class for managing player event subscriptions and responses in the GameManager
public sealed partial class GameManager : MonoBehaviour
{
    // Registers handlers for player events (coin collection, death, respawn) to trigger corresponding game actions
    void SubscribeToPlayerEvents()
    {
        player.OnCoinTake += TakeCoin;
        player.OnDeath += StopAllObjects;
        player.OnRespawn += Restart;
    }

    // Increments coin count and updates UI (called when Player collects a coin)
    void TakeCoin()
    {
        coin += 1;
        coinsCollectedThisRound += 1;
        coinText.text = coin.ToString();
    }

    // Stops Coins and Obstacles movement, cancels spawns, and updates death stats (called on Player death)
    void StopAllObjects()
    {
        foreach (Movable coin in coinsToPool) coin.Move(Movable.MoveDirection.None);
        foreach (Movable obstacle in obstaclesToPool) obstacle.Move(Movable.MoveDirection.None);
        if (spawnBirdsOption) foreach (Bird bird in birdsToPool) bird.FlyAwayAfterPlayerDeath();

        StopGameplayLoops();

        totalDeaths += 1;
        deathText.text = $"Total Deaths: {totalDeaths}\nHigh Score: {highScore}\nCoins Collected This Round: {coinsCollectedThisRound}";
        coinsCollectedThisRound = 0;
        this.Wait(0.5f, () => menu.SetActive(true));
    }

    // Disables all pooled objects, restarts gameplay loops, resets score and updates UI (called on Player respawn)
    void Restart()
    {
        foreach (Movable coin in coinsToPool) coin.gameObject.SetActive(false);
        foreach (Movable obstacle in obstaclesToPool) obstacle.gameObject.SetActive(false);
        foreach (Movable bird in birdsToPool) bird.gameObject.SetActive(false);

        StartGameplayLoops();

        currentScore = 0;
        scoreText.text = currentScore + " / " + highScore;
        newHighScore = false;
    }
}