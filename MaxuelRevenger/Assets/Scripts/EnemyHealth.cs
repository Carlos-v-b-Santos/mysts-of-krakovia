using UnityEngine;
using System.Collections;

public class EnemyHealth : MonoBehaviour
{
    // --- VARIÁVEIS DE CONFIGURAÇÃO ---
    [Header("Health Settings")]
    [SerializeField] private int maxHealth = 3;   // Vida máxima do inimigo, ajustável no Inspector.
    private int currentHealth;                  // Vida atual do inimigo durante o jogo.

    // --- REFERÊNCIAS INTERNAS ---
    private SpriteRenderer spriteRenderer;      // Referência ao componente que desenha o sprite.
    private EnemyPatrol patrolScript;           // Referência ao script de patrulha para podermos pará-lo.
    //private Animator anim;                    // Referência ao componente Animator.

    // Awake corre uma vez, antes do Start.
    void Awake()
    {
        // Define a vida inicial e "apanha" as referências aos outros componentes.
        currentHealth = maxHealth;
        spriteRenderer = GetComponent<SpriteRenderer>();
        patrolScript = GetComponent<EnemyPatrol>();
        //anim = GetComponent<Animator>();
    }

    // Método público chamado pelo script do jogador para causar dano.
    public void TakeDamage(int damageAmount)
    {
        currentHealth -= damageAmount;

        // Verifica se o dano foi fatal.
        if (currentHealth <= 0)
        {
            // Se a vida acabou, inicia a sequência de morte.
            StartCoroutine(Die());
        }
        else
        {
            // Se ainda tiver vida, toca apenas o efeito de flash.
            StartCoroutine(DamageFlashCoroutine());
        }
    }

    // Corrotina para o efeito de piscar a vermelho ao sofrer dano.
    private IEnumerator DamageFlashCoroutine()
    {
        // Ciclo para piscar duas vezes.
        for (int i = 0; i < 2; i++)
        {
            spriteRenderer.color = Color.red;
            yield return new WaitForSeconds(0.1f); // Pausa a corrotina por 0.1s.
            spriteRenderer.color = Color.white;
            yield return new WaitForSeconds(0.1f); // Pausa novamente.
        }
    }

    // Corrotina que orquestra a sequência de morte do inimigo.
    private IEnumerator Die()
    {
        // Inicia o flash de dano para o golpe final.
        StartCoroutine(DamageFlashCoroutine());

        // Para a corrotina de patrulha para que o inimigo pare de se mover.
        if (patrolScript != null)
        {
            patrolScript.StopPatrol();
        }
        // Desativa o colisor para que o corpo morto não possa mais dar ou receber dano.
        GetComponent<Collider2D>().enabled = false;
        // Congela o inimigo no lugar.
        GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Static;

        // A ser ativado na aula: dispara o gatilho da animação de morte.
        // anim.SetTrigger("die");

        // Pausa a corrotina para dar tempo à animação de morte para tocar.
        yield return new WaitForSeconds(1f);

        // Apenas no final, remove o objeto do inimigo da cena.
        Destroy(gameObject);
    }
}