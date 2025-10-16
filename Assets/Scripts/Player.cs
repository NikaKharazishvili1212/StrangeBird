using UnityEngine;
using UnityEngine.UI;

// Handles player movement, skills, and collision detection
public sealed class Player : MonoBehaviour
{
    public event System.Action OnCoinTake;
    public event System.Action OnDeath;
    public event System.Action OnRespawn;

    [SerializeField] KeyCode flapKey;
    [SerializeField] float jumpForce = 4.5f, skill2Timer = 0, skill2Cooldown = 9f;
    [SerializeField] bool isAlive = true, isInvulnerable = false;
    [SerializeField] Image skill2;
    [SerializeField] RuntimeAnimatorController[] animatorControllers;
    [SerializeField] Animator animator, skill1Animator, skill2Animator;
    [SerializeField] Rigidbody2D rb;
    [SerializeField] SpriteRenderer spriteRenderer;
    [SerializeField] AudioSource audioSource;
    [SerializeField] AudioClip[] sounds;

    void Awake()
    {
        animator.runtimeAnimatorController = animatorControllers[PlayerPrefs.GetInt("BirdSelected", 0)];
        skill2Cooldown = PlayerPrefs.GetInt("Skill2Level", 1) == 1 ? 9f : PlayerPrefs.GetInt("Skill2Level", 1) == 2 ? 8f : 7f;
        flapKey = (KeyCode)System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("FlapKey"));
    }

    void Update()
    {
        if (!isAlive) return;
        ApplyFlapAndGravity();
        AutoUseSkill2();
    }

    void ApplyFlapAndGravity()
    {
        rb.velocityY -= 12 * Time.deltaTime;  // Gravity

        if (Input.GetKeyDown(flapKey) || Input.GetMouseButtonDown(0) || (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began))
        {
            audioSource.PlayOneShot(sounds[0]);
            rb.velocity = Vector2.zero;  // Reset vertical velocity before applying the flap force
            rb.AddForce(new Vector2(0, jumpForce), ForceMode2D.Impulse);  // An upward force(Flap)
        }
    }

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

            this.Wait(2f, () =>
            {
                audioSource.PlayOneShot(sounds[3]);
                isInvulnerable = false;
                skill2Animator.Play("AnimateSkill");
                skill2.color = new Color(1, 1, 1, 1);
                spriteRenderer.color = new Color(1, 1, 1, 1);
            });
        }
    }

    // Collision detection with Obstacles and Coins
    void OnTriggerEnter2D(Collider2D other)
    {
        if (isAlive)
        {
            if (other.gameObject.CompareTag("Enemy") && !isInvulnerable) Death();
            else if (other.gameObject.CompareTag("Coin")) TakeCoin(other.gameObject);
        }
    }

    // Death by touching an Obstacle
    void Death()
    {
        audioSource.PlayOneShot(sounds[4]);
        isAlive = false;
        animator.enabled = false;
        rb.velocity = Vector2.zero;
        OnDeath?.Invoke();  // Raise the death event (GameManager reacts to it)
    }

    // Taking a Coin by touching it
    void TakeCoin(GameObject coin)
    {
        coin.SetActive(false);
        audioSource.PlayOneShot(sounds[1]);
        skill1Animator.Play("AnimateSkill");
        OnCoinTake?.Invoke();  // Raise the Coin taking event (GameManager reacts to it)
    }

    // Reset Player state on Respawn
    public void Respawn()
    {
        skill2Timer = 0;
        isAlive = true;
        animator.enabled = true;
        transform.position = new Vector3(-1.5f, 0);
        OnRespawn?.Invoke();  // Raise the respawn event (GameManager reacts to it)
    }
}