using UnityEngine;
using Unity.Netcode;
using System.Collections;

public class EnemyPatrol : NetworkBehaviour // Mude para NetworkBehaviour
{
    [Header("Patrol Points")]
    [SerializeField] private Transform pointA;
    [SerializeField] private Transform pointB;

    [Header("Enemy Settings")]
    [SerializeField] private float speed = 2f;
    [SerializeField] private float idleDuration = 2f;

    private Transform currentTarget;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Animator anim;

    public override void OnNetworkSpawn()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();

        // Apenas o servidor deve controlar a IA e o movimento do inimigo.
        if (!IsServer) return;

        currentTarget = pointB;
        StartCoroutine(PatrolRoutine());
    }

    private IEnumerator PatrolRoutine()
    {
        while (true)
        {
            anim.SetFloat("speed", 1); // Animação de andar

            while (Vector2.Distance(transform.position, currentTarget.position) > 0.5f)
            {
                Vector2 direction = (currentTarget.position - transform.position).normalized;
                rb.velocity = new Vector2(direction.x * speed, rb.velocity.y);
                FlipSprite();
                yield return null;
            }

            rb.velocity = Vector2.zero;
            anim.SetFloat("speed", 0); // Animação de parar

            yield return new WaitForSeconds(idleDuration);

            if (currentTarget == pointB) currentTarget = pointA;
            else currentTarget = pointB;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // A lógica de colisão só precisa de correr no servidor.
        if (!IsServer) return;

        if (collision.gameObject.CompareTag("Player"))
        {
            // CORREÇÃO: Pega no PlayerHealth do jogador e chama o seu ServerRpc.
            var playerHealth = collision.gameObject.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                // Pede ao servidor do JOGADOR para processar o dano.
                // O segundo argumento é o ID do atacante (este inimigo).
                playerHealth.TakeDamageServerRpc(1, NetworkObjectId);
            }
        }
    }

    private void FlipSprite()
    {
        if (rb.velocity.x > 0) spriteRenderer.flipX = false;
        else if (rb.velocity.x < 0) spriteRenderer.flipX = true;
    }
}