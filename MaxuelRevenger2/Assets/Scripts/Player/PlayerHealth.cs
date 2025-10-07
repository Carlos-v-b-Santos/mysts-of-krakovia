using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerHealth : MonoBehaviour
{
    private PlayerController player;
    private SpriteRenderer spriteRenderer;

    public int CurrentHealth { get; private set; }
    private bool isDead = false;

    private void Awake()
    {
        player = GetComponent<PlayerController>();
        spriteRenderer = player.spriteRenderer;
    }

    private void Start()
    {
        // Pega a vida máxima do ScriptableObject
        CurrentHealth = player.stats.maxHealth;
    }

    public void TakeDamage(int damageAmount, Transform enemyTransform)
    {
        if (isDead || !player.motor.CanMove) return;

        CurrentHealth -= damageAmount;
        Debug.Log("Player health: " + CurrentHealth);

        if (CurrentHealth <= 0)
        {
            StartCoroutine(DieSequence(enemyTransform));
        }
        else
        {
            StartCoroutine(player.motor.KnockbackCoroutine(enemyTransform));
            StartCoroutine(DamageFlashCoroutine());
        }
    }

    private IEnumerator DieSequence(Transform enemyTransform)
    {
        isDead = true;
        yield return StartCoroutine(player.motor.KnockbackCoroutine(enemyTransform));

        player.playerAnimator.TriggerDieAnimation();
        player.rb.velocity = Vector2.zero;
        player.enabled = false;

        yield return new WaitForSeconds(1.5f);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private IEnumerator DamageFlashCoroutine()
    {
        for (int i = 0; i < 3; i++)
        {
            spriteRenderer.color = Color.red;
            yield return new WaitForSeconds(0.1f);
            spriteRenderer.color = Color.white;
            yield return new WaitForSeconds(0.1f);
        }
    }
}