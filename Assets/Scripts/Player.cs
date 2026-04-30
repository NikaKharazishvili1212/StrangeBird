using UnityEngine;
using UnityEngine.UI;
using static Constants;

/// <summary>Handles player movement, skills, and collision detection.</summary>
public sealed class Player : MonoBehaviour
{
    public event System.Action OnCoinTake, OnDeath, OnRespawn;

    [SerializeField] KeyCode flapKey;
    [SerializeField] Image skill2;
    [SerializeField] RuntimeAnimatorController[] animatorControllers;
    [SerializeField] Animator animator, skill1Animator, skill2Animator;
    [SerializeField] Rigidbody2D rb;
    [SerializeField] SpriteRenderer spriteRenderer;
    [SerializeField] AudioSource audioSource;
    [SerializeField] AudioClip[] sounds; // 0=flap, 1=coin, 2=skill2activate, 3=skill2end, 4=death
    [SerializeField] Whoosh[] whooshVFXPool;
    float jumpForce = PlayerJumpForce;
    float skill2ShieldLevel, skill2Timer, skill2Cooldown;
    bool isAlive = true, isInvulnerable = false;

    // Load bird skin, skill cooldown, and flap key from PlayerPrefs on startup
    void Awake()
    {
        animator.runtimeAnimatorController = animatorControllers[PlayerPrefs.GetInt("BirdSelected", 0)];
        skill2ShieldLevel = PlayerPrefs.GetInt("Skill2ShieldLevel", 1);
        skill2Cooldown = skill2ShieldLevel == 0 ? Skill2Level0Cooldown : skill2ShieldLevel == 1 ? Skill2Level1Cooldown : skill2ShieldLevel == 2 ? Skill2Level2Cooldown : Skill2Level3Cooldown;
        flapKey = (KeyCode)System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("FlapKey"));
    }

    // Handle flap input and skill 2 each frame while alive
    void Update()
    {
        if (!isAlive) return;
        ApplyFlapAndGravity();
        AutoUseSkill2();
    }

    // Apply gravity each frame and handle flap input from keyboard, mouse, or touch
    void ApplyFlapAndGravity()
    {
        rb.linearVelocityY -= PlayerGravity * Time.deltaTime; // Gravity

        if (Input.GetKeyDown(flapKey) || Input.GetMouseButtonDown(0) || (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began))
        {
            audioSource.PlayOneShot(sounds[0]);
            rb.linearVelocity = Vector2.zero; // Reset vertical velocity before applying the flap force
            rb.AddForce(new Vector2(0, jumpForce), ForceMode2D.Impulse); // An upward force(Flap)

            foreach (var whooshVFX in whooshVFXPool)
            {
                if (!whooshVFX.gameObject.activeInHierarchy)
                {
                    whooshVFX.Activate();
                    whooshVFX.transform.position = transform.position - Vector3.up * 0.15f;
                    break;
                }
            }
        }
    }

    // Automatically activate skill 2 when cooldown is complete, granting temporary invulnerability
    void AutoUseSkill2()
    {
        if (skill2Timer < skill2Cooldown) skill2Timer += Time.deltaTime;
        else
        {
            audioSource.PlayOneShot(sounds[2]);
            skill2Timer = 0;
            isInvulnerable = true;
            skill2.color = new Color(1, 1, 1, 0.5f);
            spriteRenderer.color = new Color(1, 1, 1, 0.5f);

            this.Wait(Skill2InvulnerabilityDuration, () =>
            {
                audioSource.PlayOneShot(sounds[3]);
                isInvulnerable = false;
                skill2Animator.Play("AnimateSkill");
                skill2.color = new Color(1, 1, 1, 1);
                spriteRenderer.color = new Color(1, 1, 1, 1);
            });
        }
    }

    // Handle collision with obstacles (death) and coins (collection)
    void OnTriggerEnter2D(Collider2D other)
    {
        if (!isAlive) return;
        if (other.gameObject.CompareTag("Enemy") && !isInvulnerable) Death();
        else if (other.gameObject.CompareTag("Coin")) TakeCoin(other.gameObject);
    }

    // Trigger death sequence: stop movement, disable animation, notify listeners
    void Death()
    {
        audioSource.PlayOneShot(sounds[4]);
        isAlive = false;
        animator.enabled = false;
        rb.linearVelocity = Vector2.zero;
        foreach (var whooshVFX in whooshVFXPool) whooshVFX.gameObject.SetActive(false);
        OnDeath?.Invoke();
    }

    // Deactivate coin, play sound, animate skill 1, and notify listeners
    void TakeCoin(GameObject coin)
    {
        coin.SetActive(false);
        audioSource.PlayOneShot(sounds[1]);
        skill1Animator.Play("AnimateSkill");
        OnCoinTake?.Invoke();
    }

    // Reset Player state on Respawn
    public void Respawn()
    {
        skill2Timer = 0;
        isAlive = true;
        animator.enabled = true;
        transform.position = new Vector3(-1.5f, 0);
        OnRespawn?.Invoke();
    }
}