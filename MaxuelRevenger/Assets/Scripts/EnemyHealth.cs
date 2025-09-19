using UnityEngine;
using System.Collections;

public class EnemyHealth : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private int maxHealth = 3;
    private int currentHealth;

    // Referências para outros componentes
    private Animator anim;
    private SpriteRenderer spriteRenderer;
    private EnemyPatrol patrolScript;

    void Awake()
    {
        currentHealth = maxHealth;
        anim = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        patrolScript = GetComponent<EnemyPatrol>();
    }

    public void TakeDamage(int damageAmount)
    {
        currentHealth -= damageAmount;

        if (currentHealth <= 0)
        {
            StartCoroutine(Die());
        }
        else
        {
            // Se não morreu, apenas toca a rotina de flash
            StartCoroutine(DamageFlashCoroutine());
        }
    }

    private IEnumerator DamageFlashCoroutine()
    {
        // anim.SetTrigger("hurt"); // Gatilho para a animação de "tomar dano"
        for (int i = 0; i < 2; i++)
        {
            spriteRenderer.color = Color.red;
            yield return new WaitForSeconds(0.1f);
            spriteRenderer.color = Color.white;
            yield return new WaitForSeconds(0.1f);
        }
    }

    private IEnumerator Die()
    {
        // Desativa os componentes para que o inimigo pare
        if (patrolScript != null) patrolScript.enabled = false;
        GetComponent<Collider2D>().enabled = false;
        GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Static;

        // Toca a animação de morte
        // anim.SetTrigger("die"); 

        yield return new WaitForSeconds(1f); // Espera a animação de morte

        Destroy(gameObject);
    }
}