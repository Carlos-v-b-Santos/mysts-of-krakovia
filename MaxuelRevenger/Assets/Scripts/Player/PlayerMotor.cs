using System.Collections;
using UnityEngine;

public class PlayerMotor : MonoBehaviour
{
    private PlayerController player;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private PlayerInputManager inputManager;

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheckPoint;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float groundCheckRadius = 0.2f;

    [Header("Knockback")]
    [SerializeField] private float knockbackAirGravityScale = 4f;

    public bool IsGrounded { get; private set; }
    public bool CanMove { get; private set; } = true;
    private float originalGravityScale;
    private Vector2 moveInput;

    private void Awake()
    {
        player = GetComponent<PlayerController>();
        rb = player.rb;
        spriteRenderer = player.spriteRenderer;
        originalGravityScale = rb.gravityScale;
        inputManager = PlayerInputManager.GetInstance(); // Pega a instância do seu Input Manager
    }

    private void OnEnable()
    {
        // Inscreve o método Jump no evento de pulo
        GameEventsManager.Instance.inputEvents.OnJumpPressed += Jump;
        GameEventsManager.Instance.inputEvents.OnMovePressed += SetMoveDirection;
    }

    private void OnDisable()
    {
        // Limpa a inscrição para evitar erros
        GameEventsManager.Instance.inputEvents.OnJumpPressed -= Jump;
        GameEventsManager.Instance.inputEvents.OnMovePressed -= SetMoveDirection;
    }

    private void SetMoveDirection(float direction)
    {
        moveInput.x = direction;
    }

    private void FixedUpdate()
    {
        CheckIfGrounded();
        if (CanMove)
        {
            Move();
        }
    }

    private void Move()
    {
        rb.velocity = new Vector2(moveInput.x * player.stats.moveSpeed, rb.velocity.y);
        Flip(moveInput.x);
    }

    private void Flip(float moveDirection)
    {
        if (moveDirection > 0)
        {
            spriteRenderer.flipX = false;
            player.combat.attackPivot.localScale = Vector3.one;
        }
        else if (moveDirection < 0)
        {
            spriteRenderer.flipX = true;
            player.combat.attackPivot.localScale = new Vector3(-1, 1, 1);
        }
    }

    private void Jump()
    {
        if (IsGrounded && CanMove)
        {
            rb.velocity = new Vector2(rb.velocity.x, 0);
            rb.AddForce(Vector2.up * player.stats.jumpForce, ForceMode2D.Impulse);
        }
    }

    private void CheckIfGrounded()
    {
        IsGrounded = Physics2D.OverlapCircle(groundCheckPoint.position, groundCheckRadius, groundLayer);
    }

    public IEnumerator KnockbackCoroutine(Transform enemyTransform)
    {
        CanMove = false;
        rb.velocity = Vector2.zero;

        Vector2 knockbackDirection = (transform.position - enemyTransform.position).normalized;
        rb.AddForce(knockbackDirection * player.stats.knockbackForce, ForceMode2D.Impulse);

        if (!IsGrounded)
        {
            rb.gravityScale = knockbackAirGravityScale;
        }

        // Failsafe: Espera até estar no chão OU por no máximo 1.5 segundos
        float timer = 0f;
        while (!IsGrounded && timer < 1.5f)
        {
            timer += Time.deltaTime;
            yield return null;
        }

        yield return new WaitForSeconds(0.1f);

        rb.gravityScale = originalGravityScale;
        CanMove = true;
    }
}