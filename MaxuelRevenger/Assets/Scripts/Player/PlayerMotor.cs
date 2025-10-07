using UnityEngine;
using Unity.Netcode;
using System.Collections;

// Este script não precisa de ser um NetworkBehaviour, pois a sua lógica
// será controlada pelo PlayerController, que já verifica o IsOwner.
// No entanto, para acesso a propriedades de rede no futuro, pode ser útil.
public class PlayerMotor : MonoBehaviour
{
    [Header("Referências")]
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Transform attackPivot;
    [SerializeField] private PlayerStatsRuntime playerStats; // Referência para ler os stats

    [Header("Configurações de Movimento")]
    [SerializeField] private float jumpForce = 15f;

    [Header("Verificação de Chão")]
    [SerializeField] private Transform groundCheckPoint;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float groundCheckRadius = 0.2f;

    [Header("Configurações de Knockback")]
    [SerializeField] private float knockbackForce = 5f;
    [SerializeField] private float knockbackAirGravityScale = 4f;

    // Referência para o nosso script de input
    private PlayerController player;
    private PlayerInput playerInput;

    // Variáveis de estado
    private Vector2 moveInput;
    public bool isGrounded { get; private set; }
    public bool CanMove { get; private set; } = true;
    private float originalGravityScale;

    private void Awake()
    {
        // Apanha a referência para o PlayerInput que está no mesmo GameObject.
        player = GetComponent<PlayerController>();
        playerInput = GetComponent<PlayerInput>();
        originalGravityScale = rb.gravityScale;
    }

    private void OnEnable()
    {
        // Inscreve-se nos eventos LOCAIS do PlayerInput para saber quando agir.
        playerInput.OnMoveEvent += HandleMove;
        playerInput.OnJumpPressed += HandleJump;
    }

    private void OnDisable()
    {
        // Limpa a inscrição para evitar erros.
        playerInput.OnMoveEvent -= HandleMove;
        playerInput.OnJumpPressed -= HandleJump;
    }

    // FixedUpdate é o local correto para toda a lógica de física.
    private void FixedUpdate()
    {
        CheckIfGrounded();

        // Se o jogador tiver controlo, aplica o movimento.
        if (CanMove)
        {
            // Usa o atributo "speed" do PlayerStatsRuntime.
            rb.velocity = new Vector2(moveInput.x * player.stats.speed, rb.velocity.y);
        }
    }

    // Este método é chamado pelo evento OnMove do PlayerInput. A sua única função é guardar a direção.
    private void HandleMove(float direction)
    {
        if (CanMove)
        {
            moveInput.x = direction;
            Flip(direction);
        }
        else
        {
            moveInput.x = 0;
        }
    }

    // Este método é chamado pelo evento OnJumpPressed do PlayerInput.
    private void HandleJump()
    {
        // A condição para saltar é estar no chão e ter controlo.
        if (isGrounded && CanMove)
        {
            // O motor só se preocupa com a física, não com a animação.
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        }
    }

    // Inverte o sprite e o pivô de ataque com base na direção.
    private void Flip(float direction)
    {
        if (direction > 0)
        {
            spriteRenderer.flipX = false;
            attackPivot.localScale = Vector3.one;
        }
        else if (direction < 0)
        {
            spriteRenderer.flipX = true;
            attackPivot.localScale = new Vector3(-1, 1, 1);
        }
    }

    private void CheckIfGrounded()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheckPoint.position, groundCheckRadius, groundLayer);
    }

    // A corrotina de knockback, que é uma ação puramente física, vive aqui.
    public IEnumerator KnockbackCoroutine(Transform enemyTransform)
    {
        CanMove = false;
        rb.velocity = Vector2.zero;

        Vector2 knockbackDirection = (transform.position - enemyTransform.position).normalized;
        rb.AddForce(knockbackDirection * player.stats.knockbackForce, ForceMode2D.Impulse);

        if (isGrounded)
        {
            yield return new WaitForSeconds(0.4f); // Duração do "stun" no chão
        }
        else
        {
            rb.gravityScale = knockbackAirGravityScale;
            yield return new WaitForFixedUpdate();
            while (!isGrounded)
            {
                yield return new WaitForFixedUpdate();
            }
        }

        rb.gravityScale = originalGravityScale;
        CanMove = true;
    }
}