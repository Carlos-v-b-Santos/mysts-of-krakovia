using UnityEngine;
using Unity.Netcode;
using System.Collections;

// Este script n�o precisa de ser um NetworkBehaviour, pois a sua l�gica
// ser� controlada pelo PlayerController, que j� verifica o IsOwner.
// No entanto, para acesso a propriedades de rede no futuro, pode ser �til.
public class PlayerMotor : MonoBehaviour
{
    [Header("Refer�ncias")]
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Transform attackPivot;
    [SerializeField] private PlayerStatsRuntime playerStats; // Refer�ncia para ler os stats

    [Header("Configura��es de Movimento")]
    [SerializeField] private float jumpForce = 15f;

    [Header("Verifica��o de Ch�o")]
    [SerializeField] private Transform groundCheckPoint;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float groundCheckRadius = 0.2f;

    [Header("Configura��es de Knockback")]
    [SerializeField] private float knockbackForce = 5f;
    [SerializeField] private float knockbackAirGravityScale = 4f;

    // Refer�ncia para o nosso script de input
    private PlayerController player;
    private PlayerInput playerInput;

    // Vari�veis de estado
    private Vector2 moveInput;
    public bool isGrounded { get; private set; }
    public bool CanMove { get; private set; } = true;
    private float originalGravityScale;

    private void Awake()
    {
        // Apanha a refer�ncia para o PlayerInput que est� no mesmo GameObject.
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
        // Limpa a inscri��o para evitar erros.
        playerInput.OnMoveEvent -= HandleMove;
        playerInput.OnJumpPressed -= HandleJump;
    }

    // FixedUpdate � o local correto para toda a l�gica de f�sica.
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

    // Este m�todo � chamado pelo evento OnMove do PlayerInput. A sua �nica fun��o � guardar a dire��o.
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

    // Este m�todo � chamado pelo evento OnJumpPressed do PlayerInput.
    private void HandleJump()
    {
        // A condi��o para saltar � estar no ch�o e ter controlo.
        if (isGrounded && CanMove)
        {
            // O motor s� se preocupa com a f�sica, n�o com a anima��o.
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        }
    }

    // Inverte o sprite e o piv� de ataque com base na dire��o.
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

    // A corrotina de knockback, que � uma a��o puramente f�sica, vive aqui.
    public IEnumerator KnockbackCoroutine(Transform enemyTransform)
    {
        CanMove = false;
        rb.velocity = Vector2.zero;

        Vector2 knockbackDirection = (transform.position - enemyTransform.position).normalized;
        rb.AddForce(knockbackDirection * player.stats.knockbackForce, ForceMode2D.Impulse);

        if (isGrounded)
        {
            yield return new WaitForSeconds(0.4f); // Dura��o do "stun" no ch�o
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