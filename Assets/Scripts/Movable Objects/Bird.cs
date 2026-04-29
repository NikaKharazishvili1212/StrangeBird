using TMPro;
using UnityEngine;
using static Utils;
using static Constants;

/// <summary>Handles bird movement, random visual variations, and chat bubble display.</summary>
public sealed class Bird : Movable
{
    [SerializeField] GameObject chatBubble;
    [SerializeField] TextMeshPro chatText;
    [SerializeField] AudioSource audioSource;
    [SerializeField] AudioClip[] birdChatSounds;
    [SerializeField] Animator animator;
    [SerializeField] RuntimeAnimatorController[] birdVariationAnimators;
    static float moveSpeed;

    // Collection of random bird-themed chat messages that can be displayed by flying birds
    static readonly string[] BirdChatMessages = new string[]
    {
        "What's quackin', good lookin'?", "Hey, wingman!", "Tweet dreams!", "Fly high, don't be shy!",
        "Keep calm and chirp on!", "You're eggcellent!", "Winging it today?", "What the flock!",
        "Peck on, peck off.", "Just winging by!", "Spread your wings and fly!", "You're tweet-tastic!",
        "Flying solo today?", "Keep your beak up!", "Chirp happens!", "A little birdie told me...",
        "Flap it out!", "Life's a chirp!", "Eggciting times ahead!", "What's up, beak face?",
        "You're so fly!", "Bird is the word!", "Let's get this bread!", "Shake your tail feathers!",
        "Wing it like a boss!", "Feathered and fabulous!", "Stay chirpy!", "You're the tweetest!",
        "Flap till you drop!", "Peckish, aren't we?", "Squawk to the walk!", "Catch you on the fly!",
        "Don't ruffle my feathers!", "High-flying fun!", "Quirky chirp!", "Feather in your cap!",
        "Keep flapping!", "Feathered friends forever!", "Tweet it out!", "Birds of paradise!",
        "Squawk and awe!", "Up, up, and away!", "Beak sneak!", "Chirp-tastic!",
        "Flap happy!", "Bird-brained fun!", "Fluff and stuff!"
    };

    // Bird's moving speed depends on game difficulty
    void Awake() => moveSpeed = gameManager.difficulty == 0 ? EasyBirdSpeed : gameManager.difficulty == 1 ? MediumBirdSpeed : HardBirdSpeed;

    // Initialize bird on spawn: reposition, randomize look, move, and maybe show chat
    void OnEnable()
    {
        TeleportToStartingPosition();
        LoadRandomBirdVariation();
        Move();
        ShowChatMessage();
    }

    // Loads a random bird visual variation from available animators
    void LoadRandomBirdVariation() => animator.runtimeAnimatorController = birdVariationAnimators[Random.Range(0, birdVariationAnimators.Length)];

    // Chance to show chat bubble with random message
    void ShowChatMessage()
    {
        if (PercentChanceSuccess(BirdChatChance))
        {
            audioSource.PlayOneShot(birdChatSounds[Random.Range(0, birdChatSounds.Length)]);
            chatBubble.SetActive(true);
            chatText.text = BirdChatMessages[Random.Range(0, BirdChatMessages.Length)];
        }
        else chatBubble.SetActive(false);
    }

    // Teleport to the right side of the screen with random Y position (for pooling)
    public override void TeleportToStartingPosition() => transform.position = new Vector2(BirdSpawnX, Random.Range(-SpawnY, SpawnY));

    // Chance to move right or left
    public override void Move()
    {
        if (PercentChanceSuccess(BirdMoveRightChance))
        {
            rb.linearVelocity = new Vector2(moveSpeed, 0f);
            transform.rotation = Quaternion.Euler(0f, 0f, 0f);
            chatText.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
        }
        else
        {
            rb.linearVelocity = new Vector2(-1f, 0f);
            transform.rotation = Quaternion.Euler(0f, 180f, 0f);
            chatText.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
        }
    }

    // Called when player dies to make birds moving right slowly, fly away
    public void FlyAwayAfterPlayerDeath() { if (rb.linearVelocityX == -1f) rb.linearVelocity = new Vector2(-moveSpeed, 0f); }

    // This guy never needs Stop() method
    public override void Stop() => throw new System.NotImplementedException("This method must be overridden in derived classes.");
}