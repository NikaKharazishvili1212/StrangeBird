using TMPro;
using UnityEngine;
using System.Collections.Generic;
using static Utils;
using static Constants;
using Unity.VisualScripting;

/// <summary>Handles bird movement, random visual variations, and chat bubble display.</summary>
public sealed class Bird : Movable
{
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

    // Depletes as birds chat; refills from BirdChatMessages when empty so no message repeats until all are exhausted
    static List<string> chatPool = new();

    string TakeRandomChat()
    {
        if (chatPool.Count == 0) chatPool.AddRange(BirdChatMessages);
        string msg = GetRandomElement(chatPool);
        chatPool.Remove(msg);
        return msg;
    }

    // Bird's moving speed depends on game speed
    void Awake() => moveSpeed = GameManager.Instance.GetGameSpeed() == 0 ? SlowBirdSpeed : GameManager.Instance.GetGameSpeed() == 1 ? MediumBirdSpeed : FastBirdSpeed;

    void Update() => DeactivateOnLeavingScreen();

    // Initialize bird on spawn: reposition, randomize look, move, and maybe show chat
    void OnEnable()
    {
        TeleportToStartingPosition();
        LoadRandomBirdVariation();
        Move();
    }

    // Loads a random bird visual variation from available animators
    void LoadRandomBirdVariation() => animator.runtimeAnimatorController = birdVariationAnimators[Random.Range(0, birdVariationAnimators.Length)];

    // Deactivates on leaving screen to be pooled again later
    public override void DeactivateOnLeavingScreen() { if (transform.position.x <= -6 || transform.position.x >= 7) gameObject.SetActive(false); }

    // Teleport to the right side of the screen with random Y position (for pooling)
    public override void TeleportToStartingPosition() => transform.position = new Vector2(BirdSpawnX, Random.Range(-BirdSpawnY, BirdSpawnY));

    // Chance to move right or left
    public override void Move()
    {
        chatText.gameObject.SetActive(false);
        if (PercentChance(BirdMoveRightChance))
        {
            rb.linearVelocityX = -1;
            transform.rotation = Quaternion.Euler(0, 180, 0);

            // Chance to show chat
            if (PercentChance(BirdChatChance))
            {
                this.Wait(BirdChatDelay, () =>
                {
                    if (!GameManager.Instance.player.isAlive) return; // If player is dead, then don't talk to him
                    audioSource.PlayOneShot(GetRandomElement(birdChatSounds));
                    chatText.gameObject.SetActive(true);
                    chatText.text = TakeRandomChat();
                });
            }
        }
        else
        {
            rb.linearVelocityX = moveSpeed;
            transform.rotation = Quaternion.Euler(0, 0, 0);
            chatText.gameObject.SetActive(false);
        }
    }

    // Called when player dies to make birds moving right slowly, fly away
    public void FlyAwayAfterPlayerDeath() { if (rb.linearVelocityX == -1) rb.linearVelocity = new Vector2(-moveSpeed, 0); }

    public override void Stop() => throw new System.NotImplementedException("Bird never needs Stop() method.");
}