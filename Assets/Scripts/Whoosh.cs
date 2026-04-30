using UnityEngine;
using static Constants;

/// <summary>Pooled by player on each jump; has it's own lifespan.</summary>
public class Whoosh : MonoBehaviour
{
    [SerializeField] Animator animator;

    // Pooled by player
    public void Activate()
    {
        gameObject.SetActive(true);
        animator.Play(0); // There's only one animation
        this.Wait(WhooshLifespan, () => gameObject.SetActive(false));
    }

    // Move back
    void Update() => transform.position += Vector3.left * (WhooshSpeed * Time.deltaTime);
}