using UnityEngine;
using System.Collections;

public class EnemyPatrol : MonoBehaviour
{
    // --- VARI�VEIS DE CONFIGURA��O (AJUST�VEIS NO INSPECTOR) ---
    [Header("Patrol Points")]
    [SerializeField] private Transform pointA;      // Ponto de in�cio da patrulha.
    [SerializeField] private Transform pointB;      // Ponto final da patrulha.

    [Header("Enemy Settings")]
    [SerializeField] private float speed = 2f;        // Velocidade de movimento do inimigo.
    [SerializeField] private float idleDuration = 2f; // Tempo que o inimigo fica parado em cada ponto.

    // --- VARI�VEIS DE ESTADO INTERNAS ---
    private Transform currentTarget;            // Guarda o alvo atual (A ou B).
    private Rigidbody2D rb;                     // Refer�ncia ao componente de f�sica.
    private SpriteRenderer spriteRenderer;      // Refer�ncia ao componente que desenha o sprite.
    private Coroutine runningPatrolRoutine;     // Refer�ncia � corrotina de patrulha para podermos par�-la.
    //private Animator anim;                    // Refer�ncia ao componente Animator.

    // Start corre uma vez no in�cio, ap�s o Awake.
    void Start()
    {
        // "Apanha" os componentes que est�o no mesmo GameObject.
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        //anim = GetComponent<Animator>();

        // Define o alvo inicial e inicia a rotina de patrulha.
        currentTarget = pointB;
        runningPatrolRoutine = StartCoroutine(PatrolRoutine());
    }

    // Corrotina que controla todo o ciclo de movimento e pausas do inimigo.
    private IEnumerator PatrolRoutine()
    {
        // Um loop infinito garante que o inimigo patrulhe para sempre.
        while (true)
        {
            // --- Fase de Movimento ---
            // A ser ativado na aula: informa o Animator para tocar a anima��o de "correr".
            //anim.SetFloat("speed", speed);

            // Um loop interno que continua enquanto o inimigo n�o chegar ao seu alvo.
            while (Vector2.Distance(transform.position, currentTarget.position) > 0.5f)
            {
                // Calcula a dire��o, aplica a velocidade e vira o sprite.
                Vector2 direction = (currentTarget.position - transform.position).normalized;
                rb.velocity = new Vector2(direction.x * speed, rb.velocity.y);
                FlipSprite();
                // Pausa a corrotina at� o pr�ximo frame, para n�o bloquear o jogo.
                yield return null;
            }

            // --- Fase de Pausa (Idle) ---
            rb.velocity = Vector2.zero; // Para o inimigo.
            // A ser ativado na aula: informa o Animator para tocar a anima��o de "parado".
            //anim.SetFloat("speed", 0);

            // Pausa a corrotina pela dura��o definida, fazendo o inimigo esperar.
            yield return new WaitForSeconds(idleDuration);

            // --- Troca de Alvo ---
            // Inverte o alvo para a pr�xima itera��o do loop.
            if (currentTarget == pointB)
            {
                currentTarget = pointA;
            }
            else
            {
                currentTarget = pointB;
            }
        }
    }

    // M�todo que causa dano ao jogador ao tocar nele.
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Verifica se o objeto com que colidiu tem a tag "Player".
        if (collision.gameObject.CompareTag("Player"))
        {
            // Pega a refer�ncia do script do jogador para chamar o m�todo de dano.
            PlayerController player = collision.gameObject.GetComponent<PlayerController>();
            if (player != null)
            {
                player.health.TakeDamage(1, transform);
            }
        }
    }

    // M�todo p�blico que para a corrotina de patrulha. Chamado pelo script EnemyHealth quando o inimigo morre.
    public void StopPatrol()
    {
        if (runningPatrolRoutine != null)
        {
            StopCoroutine(runningPatrolRoutine);
        }
    }

    // Vira o sprite do inimigo com base na dire��o da sua velocidade horizontal.
    private void FlipSprite()
    {
        // Lembre-se que esta l�gica pode precisar de ser invertida (true/false) dependendo da dire��o original do seu sprite.
        if (rb.velocity.x > 0)
        {
            spriteRenderer.flipX = false; // Se a velocidade � positiva, olha para a direita.
        }
        else if (rb.velocity.x < 0)
        {
            spriteRenderer.flipX = true; // Se a velocidade � negativa, olha para a esquerda.
        }
    }
}