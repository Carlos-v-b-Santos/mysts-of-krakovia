using UnityEngine;
using System.Collections;

public class EnemyHealth : MonoBehaviour, IDamageable
{
    // --- VARI�VEIS DE CONFIGURA��O ---
    [Header("Health Settings")]
    [SerializeField] private int maxHealth = 3;   // Vida m�xima do inimigo, ajust�vel no Inspector.
    private int currentHealth;                  // Vida atual do inimigo durante o jogo.
    [SerializeField] private int xpReward = 20;   // Quantidade de XP concedida ao jogador ao derrotar este inimigo.
    // --- REFER�NCIAS INTERNAS ---
    private SpriteRenderer spriteRenderer;      // Refer�ncia ao componente que desenha o sprite.
    private EnemyPatrol patrolScript;           // Refer�ncia ao script de patrulha para podermos par�-lo.
    //private Animator anim;                    // Refer�ncia ao componente Animator.

    // Awake corre uma vez, antes do Start.
    void Awake()
    {
        // Define a vida inicial e "apanha" as refer�ncias aos outros componentes.
        currentHealth = maxHealth;
        spriteRenderer = GetComponent<SpriteRenderer>();
        patrolScript = GetComponent<EnemyPatrol>();
        //anim = GetComponent<Animator>();
    }

    // M�todo p�blico chamado pelo script do jogador para causar dano.
    public void TakeDamage(int damageAmount, int ownerId)
    {
        currentHealth -= damageAmount;
        Debug.Log($"{gameObject.name} recebeu {damageAmount} de dano. HP restante: {currentHealth}");

        // Verifica se o dano foi fatal.
        if (currentHealth <= 0)
        {
            // Se a vida acabou, inicia a sequ�ncia de morte.
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

    // Corrotina que orquestra a sequ�ncia de morte do inimigo.
    private IEnumerator Die()
    {
        // Inicia o flash de dano para o golpe final.
        StartCoroutine(DamageFlashCoroutine());

        // Para a corrotina de patrulha para que o inimigo pare de se mover.
        if (patrolScript != null)
        {
            patrolScript.StopPatrol();
        }
        // Desativa o colisor para que o corpo morto n�o possa mais dar ou receber dano.
        GetComponent<Collider2D>().enabled = false;
        // Congela o inimigo no lugar.
        GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Static;

        // A ser ativado na aula: dispara o gatilho da anima��o de morte.
        // anim.SetTrigger("die");

        // Pausa a corrotina para dar tempo � anima��o de morte para tocar.
        yield return new WaitForSeconds(1f);

        // Apenas no final, remove o objeto do inimigo da cena.
        GameEventsManager.Instance.playerEvents.XPReceived(xpReward);
        Destroy(gameObject);
    }
}