using UnityEngine;

public class EnemyPatrol : MonoBehaviour
{
    [Header("Patrol Points")]
    [SerializeField] private Transform pointA;
    [SerializeField] private Transform pointB;

    [Header("Enemy Settings")]
    [SerializeField] private float speed = 2f;

    private Transform currentTarget;
    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        currentTarget = pointB; // Come�a a mover-se em dire��o ao ponto B
    }

    void Update()
    {
        // Calcula a dire��o para o alvo
        Vector2 direction = (currentTarget.position - transform.position).normalized;

        // Move o Rigidbody na dire��o calculada
        rb.velocity = new Vector2(direction.x * speed, rb.velocity.y);

        // Verifica se chegou perto do alvo para mudar de dire��o
        if (Vector2.Distance(transform.position, currentTarget.position) < 0.5f)
        {
            // Se o alvo atual � o ponto B, muda para o ponto A, e vice-versa
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

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Verifica se o objeto com o qual colidimos tem a tag "Player"
        if (collision.gameObject.CompareTag("Player"))
        {
            // Tenta obter o componente PlayerController do objeto do jogador
            PlayerController player = collision.gameObject.GetComponent<PlayerController>();

            // Verifica se o componente foi encontrado (para evitar erros)
            if (player != null)
            {
                // Chama o m�todo p�blico TakeDamage no script do jogador, causando 1 de dano
                player.TakeDamage(1, transform);
            }
        }
    }
}