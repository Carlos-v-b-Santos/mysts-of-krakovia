using UnityEngine;
using Unity.Netcode;
using System.Collections;

public class PlayerMotor : NetworkBehaviour // MUDAN�A 1
{
    [Header("Refer�ncias")]
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Transform attackPivot;
    [SerializeField] private PlayerStatsRuntime playerStats;
    [SerializeField] private Animator anim;

    [Header("Configura��es de Movimento")]
    [SerializeField] private float jumpForce = 15f;

    [Header("Verifica��o de Ch�o")]
    [SerializeField] private Transform groundCheckPoint;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float groundCheckRadius = 0.2f;

    [Header("Configura��es de Knockback")]
    [SerializeField] private float knockbackForce = 5f;
    [SerializeField] private float knockbackAirGravityScale = 4f;

    private PlayerController player;
    private PlayerInput playerInput;

    public bool isGrounded { get; private set; }
    public NetworkVariable<bool> canAcceptInput = new NetworkVariable<bool>(true, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    private float originalGravityScale;

    // MUDAN�A 2: Vari�vel de rede para sincronizar a dire��o
    private NetworkVariable<bool> isFacingRight = new NetworkVariable<bool>(true, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    private void Awake()
    {
        player = GetComponent<PlayerController>();
        playerInput = GetComponent<PlayerInput>();
        originalGravityScale = rb.gravityScale;
        anim = GetComponent<Animator>();
    }
   
    private void FixedUpdate()
    {
        // A verifica��o de ch�o � uma informa��o local que o Animator precisa.
        // � seguro que cada cliente verifique o seu pr�prio estado de "grounded"
        // para que as anima��es fiquem corretas e responsivas.
        if (IsOwner)
        {
            CheckIfGrounded();
        }

        // Esta parte � para o SERVIDOR verificar o ch�o e controlar o Animator de todos.
        if (IsServer)
        {
            CheckIfGrounded();
            anim.SetBool("isJumping", !isGrounded);
        }
    }
    private void OnEnable()
    {
        // A inscri��o nos eventos locais continua igual. O que muda � o que os Handlers fazem.
        playerInput.OnMoveEvent += HandleMove;
        playerInput.OnJumpPressed += HandleJump;
    }

    private void OnDisable()
    {
        playerInput.OnMoveEvent -= HandleMove;
        playerInput.OnJumpPressed -= HandleJump;
    }

    private void Update()
    {
        // A fun��o do Update agora � garantir que o estado visual (flip do sprite) esteja sincronizado em todos os clientes.
        if (isFacingRight.Value)
        {
            spriteRenderer.flipX = false;
            attackPivot.localScale = Vector3.one;
        }
        else
        {
            spriteRenderer.flipX = true;
            attackPivot.localScale = new Vector3(-1, 1, 1);
        }
    }

    // MUDAN�A 3: Handlers agora chamam ServerRPCs em vez de agir localmente
    private void HandleMove(float direction)
    {
        if (canAcceptInput.Value)
        {
            RequestMoveServerRpc(direction);
        }
        else
        {
            RequestMoveServerRpc(0);
        }
    }

    private void HandleJump()
    {
        if (canAcceptInput.Value)
        {
            RequestJumpServerRpc();
        }
    }

    [ServerRpc]
    private void RequestMoveServerRpc(float direction)
    {
        rb.velocity = new Vector2(direction * player.stats.speed, rb.velocity.y);

        if (direction > 0.1f) isFacingRight.Value = true;
        else if (direction < -0.1f) isFacingRight.Value = false;

        // O servidor define o par�metro de velocidade no Animator.
        anim.SetFloat("speed", Mathf.Abs(direction));
    }

    [ServerRpc]
    private void RequestJumpServerRpc()
    {
        CheckIfGrounded();
        if (isGrounded && canAcceptInput.Value)
        {
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        }
    }
    [ClientRpc]
    public void PlayKnockbackFeedbackClientRpc()
    {
        // ESTE C�DIGO EXECUTA EM TODOS OS CLIENTES
        Debug.Log("Feedback de knockback ativado!");
        // Futuramente, aqui voc� pode chamar o PlayerAnimator para tocar a anima��o "Hurt"
        player.playerAnimator.TriggerHurtAnimation();
        // ou tocar um som de dano.
    }
    private void CheckIfGrounded()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheckPoint.position, groundCheckRadius, groundLayer);
    }

    // O Knockback precisa ser repensado para rede, mas por agora vamos mant�-lo.
    // Idealmente, seria um ClientRpc chamado pelo servidor.
    // 1. O PONTO DE ENTRADA P�BLICO (CHAMADO POR OUTROS SCRIPTS NO SERVIDOR)
    public void ApplyKnockback(Transform enemyTransform)
    {
        // Cl�usula de guarda: Apenas o servidor pode iniciar um knockback.
        if (!IsServer) return;

        // Inicia a coroutine que vai gerir a l�gica do knockback no servidor.
        StartCoroutine(ServerKnockbackCoroutine(enemyTransform.position));

        // Comanda a todos os clientes para mostrarem o feedback visual.
        PlayKnockbackFeedbackClientRpc();
    }

    // 2. A COROUTINE QUE EXECUTA A L�GICA (APENAS NO SERVIDOR)
    private IEnumerator ServerKnockbackCoroutine(Vector3 enemyPosition)
    {
        // Remove o controlo do jogador (este valor ser� sincronizado para o cliente)
        canAcceptInput.Value = false;
        rb.velocity = Vector2.zero;

        // Calcula e aplica a for�a da f�sica
        Vector2 knockbackDirection = (transform.position - enemyPosition).normalized;
        rb.AddForce(knockbackDirection * knockbackForce, ForceMode2D.Impulse);

        CheckIfGrounded();
        if (isGrounded)
        {
            yield return new WaitForSeconds(0.4f); // Dura��o do "stun" no ch�o
        }
        else // L�gica para esperar at� tocar no ch�o se estiver no ar
        {
            rb.gravityScale = knockbackAirGravityScale;
            yield return new WaitForFixedUpdate();
            while (!isGrounded)
            {
                CheckIfGrounded();
                yield return new WaitForFixedUpdate();
            }
        }

        // Garante que a velocidade zera antes de devolver o controlo
        rb.velocity = Vector2.zero;
        rb.gravityScale = originalGravityScale;

        // Devolve o controlo ao jogador
        canAcceptInput.Value = true;
    }
}