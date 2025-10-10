using UnityEngine;
using Unity.Netcode;

// Este script controla o comportamento do inimigo, alternando entre patrulha e perseguição.
// Toda a lógica de decisão e movimento é executada apenas no servidor.
public class EnemyController : NetworkBehaviour
{
    // Define os possíveis comportamentos (estados) da nossa IA.
    private enum AIState
    {
        Patrolling,
        Chasing
    }

    [Header("State")]
    private AIState currentState;

    [Header("Patrol Settings")]
    [SerializeField] private Transform pointA;
    [SerializeField] private Transform pointB;

    [Header("AI Settings")]
    [SerializeField] private float patrolSpeed = 2f;
    [SerializeField] private float chaseSpeed = 3.5f;
    [SerializeField] private float aggroRadius = 6f;
    [SerializeField] private LayerMask playerLayer;

    // Referências de Componentes
    private Transform currentPatrolTarget;
    private Transform playerTarget;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Animator anim;

    // Awake é chamado antes de qualquer outra função, ideal para referências iniciais.
    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
    }

    // OnNetworkSpawn é o "Start" para objetos de rede.
    public override void OnNetworkSpawn()
    {
        // A lógica de IA só precisa de ser configurada e executada no servidor.
        if (!IsServer) return;

        // A configuração do alvo foi movida para o método SetPatrolPoints
        // para evitar erros de referência nula.
    }

    // ++ NOVO MÉTODO ++
    // Este método público permite que o Spawner configure os pontos de patrulha
    // no momento da criação do inimigo.
    public void SetPatrolPoints(Transform newPointA, Transform newPointB)
    {
        pointA = newPointA;
        pointB = newPointB;

        // Agora que garantimos que os pontos existem, definimos o alvo inicial.
        currentState = AIState.Patrolling;
        currentPatrolTarget = pointB;
    }


    // Update é chamado a cada frame.
    private void Update()
    {
        // Garante que o "cérebro" do inimigo só funcione no servidor.
        if (!IsServer) return;

        // Se o inimigo não tiver um alvo de patrulha, não faz nada.
        // (Cláusula de segurança adicional)
        if (currentPatrolTarget == null) return;

        // Máquina de estados: decide qual comportamento executar com base no estado atual.
        switch (currentState)
        {
            case AIState.Patrolling:
                HandlePatrollingState();
                CheckForPlayer(); // Verifica se um jogador se aproximou.
                break;

            case AIState.Chasing:
                HandleChasingState();
                break;
        }
    }

    // Lógica para quando o inimigo está a patrulhar.
    private void HandlePatrollingState()
    {
        anim.SetFloat("speed", patrolSpeed); // Usa a velocidade de patrulha na animação.

        // Se estiver longe do alvo, move-se em direção a ele.
        if (Vector2.Distance(transform.position, currentPatrolTarget.position) > 0.5f)
        {
            Vector2 direction = (currentPatrolTarget.position - transform.position).normalized;
            rb.velocity = new Vector2(direction.x * patrolSpeed, rb.velocity.y);
            FlipTowardsDirection(direction);
        }
        else // Se chegou ao alvo, para e troca de direção.
        {
            rb.velocity = Vector2.zero;
            anim.SetFloat("speed", 0);
            currentPatrolTarget = (currentPatrolTarget == pointB) ? pointA : pointB;
        }
    }

    // Lógica para quando o inimigo está a perseguir um jogador.
    private void HandleChasingState()
    {
        if (playerTarget == null)
        {
            // Se perdeu o alvo, volta a patrulhar.
            currentState = AIState.Patrolling;
            return;
        }

        anim.SetFloat("speed", chaseSpeed); // Usa a velocidade de perseguição na animação.

        Vector2 direction = (playerTarget.position - transform.position).normalized;
        rb.velocity = new Vector2(direction.x * chaseSpeed, rb.velocity.y);
        FlipTowardsDirection(direction);

        // Condição para desistir da perseguição se o jogador fugir para muito longe.
        if (Vector2.Distance(transform.position, playerTarget.position) > aggroRadius * 1.5f)
        {
            playerTarget = null;
            currentState = AIState.Patrolling;
        }
    }

    // Verifica se há jogadores dentro do raio de agressividade.
    private void CheckForPlayer()
    {
        Collider2D playerCollider = Physics2D.OverlapCircle(transform.position, aggroRadius, playerLayer);

        if (playerCollider != null)
        {
            Debug.Log("Jogador detetado! A mudar para o estado de perseguição.");
            playerTarget = playerCollider.transform; // Define o jogador como o novo alvo.
            currentState = AIState.Chasing;           // Muda o estado da IA.
        }
    }

    // Lógica para causar dano ao tocar no jogador (mantida do seu script original).
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!IsServer) return;

        if (collision.gameObject.CompareTag("Player"))
        {
            var playerHealth = collision.gameObject.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                // Pede ao servidor do JOGADOR para processar o dano.
                // O segundo argumento é o ID do atacante (este inimigo).
                playerHealth.TakeDamageServerRpc(1, NetworkObjectId);
            }
        }
    }

    // Inverte a direção do sprite com base na direção do movimento.
    private void FlipTowardsDirection(Vector2 direction)
    {
        if (direction.x > 0.01f)
        {
            spriteRenderer.flipX = false;
        }
        else if (direction.x < -0.01f)
        {
            spriteRenderer.flipX = true;
        }
    }
}