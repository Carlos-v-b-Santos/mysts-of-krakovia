using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;


public class PlayerController : MonoBehaviour
{
    // --- VARIÁVEIS DE CONFIGURAÇÃO (AJUSTÁVEIS NO INSPECTOR) ---
    // [Header] cria um título no Inspector para organização.
    // [SerializeField] expõe uma variável privada no Inspector.

    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 8f;      // Velocidade de movimento do jogador.
    [SerializeField] private float jumpForce = 6f;     // Força do impulso do pulo.

    [Header("Ground Check Settings")]
    [SerializeField] private Transform groundCheckPoint;  // Ponto de referência nos "pés" do jogador para detetar o chão.
    [SerializeField] private LayerMask groundLayer;       // Define o que é considerado "chão" para o nosso sensor.
    [SerializeField] private float groundCheckRadius = 0.2f; // Raio do círculo do sensor de chão.

    [Header("Life System")]
    [SerializeField] private int maxHealth = 3;           // Vida máxima do jogador.
    [SerializeField] private float knockbackForce = 5f;     // Força do empurrão ao sofrer dano.
    [SerializeField] private float knockbackAirGravityScale = 4f; // Gravidade extra aplicada durante o knockback aéreo para uma queda mais rápida.

    [Header("Attack Settings")]
    [SerializeField] private Animator slashAnimator;    // Referência ao Animator do efeito de corte.
    [SerializeField] private Transform attackPoint;     // Ponto de referência de onde o ataque se origina.
    [SerializeField] private float attackRange = 2.3f;    // Raio do círculo da hitbox do ataque.
    [SerializeField] private LayerMask enemyLayer;      // Define o que é considerado um "inimigo" para o nosso ataque.
    [SerializeField] private float attackCooldown = 0.5f; // Tempo de espera entre ataques.
    [SerializeField] private Transform attackPivot;     // Pivô que segura os objetos de ataque para virar com o jogador.

    // --- VARIÁVEIS DE ESTADO INTERNAS (A "MEMÓRIA" DO JOGADOR) ---
    private int currentHealth;              // Vida atual durante o jogo.
    private float originalGravityScale;     // Guarda a gravidade normal para a podermos restaurar após o knockback.
    private bool canAttack = true;          // Controla se o jogador pode atacar.
    private Rigidbody2D rb;                 // Referência ao componente de física.
    private PlayerControls playerControls;  // Referência ao nosso mapa de inputs.
    // private Animator anim;                  // Referência ao componente Animator do jogador.
    private SpriteRenderer spriteRenderer;  // Referência ao componente que desenha o sprite.
    private Vector2 moveInput;              // Guarda a direção do input de movimento.
    private bool isGrounded;                // Está a tocar no chão?
    private bool canMove = true;            // Pode mover-se?
    private bool isDead = false;            // Está morto?
    private bool isInvincible = false;      // Está invencível? (Ainda não implementado, mas preparado)
    private bool isJumping;                 // Está a pular? (Usado pelo Animator)


    // Awake corre uma vez, antes do Start. Ideal para inicializar referências.
    private void Awake()
    {
        // "Apanha" os componentes que estão no mesmo GameObject.
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        //anim = GetComponent<Animator>();

        // Cria e ativa o nosso mapa de inputs.
        playerControls = new PlayerControls();

        // Define a vida inicial.
        currentHealth = maxHealth;
    }

    // OnEnable corre quando o objeto é ativado. Perfeito para "inscrever-se" em eventos.
    private void OnEnable()
    {
        playerControls.Gameplay.Enable();
        // Diz ao sistema de input: "Quando a ação 'Jump' for executada, chame o método Jump()".
        playerControls.Gameplay.Jump.performed += Jump;
        playerControls.Gameplay.Attack.performed += Attack;
    }

    // OnDisable corre quando o objeto é desativado. Limpamos as inscrições para evitar erros.
    private void OnDisable()
    {
        playerControls.Gameplay.Disable();
        playerControls.Gameplay.Jump.performed -= Jump;
        playerControls.Gameplay.Attack.performed -= Attack;
    }

    // Update corre a cada frame. Ideal para ler inputs e lógica visual.
    private void Update()
    {
        // Apenas lê o input de movimento se o jogador tiver controlo.
        if (canMove)
        {
            moveInput.x = playerControls.Gameplay.Move.ReadValue<float>();
        }
        else
        {
            moveInput.x = 0;
        }

        // Lógica para virar o sprite do jogador e o pivô do ataque.
        if (moveInput.x > 0)
        {
            spriteRenderer.flipX = false;
            attackPivot.localScale = Vector3.one;
        }
        else if (moveInput.x < 0)
        {
            spriteRenderer.flipX = true;
            attackPivot.localScale = new Vector3(-1, 1, 1);
        }

        // Comunicação com o Animator (a ser ativada na aula).
        //anim.SetFloat("speed", Mathf.Abs(moveInput.x));
        //anim.SetFloat("velocityY", rb.velocity.y);
    }

    // FixedUpdate corre a um ritmo fixo, sincronizado com a física.
    private void FixedUpdate()
    {
        CheckIfGrounded();

        // Apenas aplica o movimento se o jogador tiver controlo.
        if (canMove)
        {
            // Aplicamos o movimento diretamente à velocidade do Rigidbody.
            rb.velocity = new Vector2(moveInput.x * moveSpeed, rb.velocity.y);
        }

        //anim.SetBool("isJumping", !isGrounded);
    }


    // Método chamado pelo evento de input "Jump".
    private void Jump(InputAction.CallbackContext context)
    {
        // A condição impede o "pulo duplo".
        if (isGrounded)
        {
            //anim.SetTrigger("jump");
            // Adiciona uma força vertical instantânea.
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        }
    }

    // Verifica se o "sensor de pés" está a tocar em algo na layer "Ground".
    private void CheckIfGrounded()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheckPoint.position, groundCheckRadius, groundLayer);
    }

    // Método público que pode ser chamado por outros scripts (como o do inimigo).
    public void TakeDamage(int damageAmount, Transform enemyTransform)
    {
        // Cláusula de guarda: ignora o dano se o jogador já estiver invencível ou morto.
        if (isInvincible || isDead || !canMove) return;

        currentHealth -= damageAmount;
        Debug.Log("Player tomou dano! Vida atual: " + currentHealth);

        if (currentHealth <= 0)
        {
            // Se o dano for fatal, inicia a sequência de morte com knockback.
            StartCoroutine(FatalHitSequence(enemyTransform));
        }
        else
        {
            // Se não for fatal, inicia o flash e o knockback.
            StartCoroutine(DamageFlashCoroutine());
            StartCoroutine(KnockbackCoroutine(enemyTransform));
        }
    }

    // Corrotina que orquestra a sequência de knockback e depois a de morte.
    private IEnumerator FatalHitSequence(Transform enemyTransform)
    {
        StartCoroutine(DamageFlashCoroutine());
        // yield return espera que a corrotina de knockback termine completamente.
        yield return StartCoroutine(KnockbackCoroutine(enemyTransform));
        // Só depois de o knockback terminar, inicia a corrotina de morte.
        StartCoroutine(Die());
    }

    // Corrotina que lida com o knockback e a perda de controlo.
    private IEnumerator KnockbackCoroutine(Transform enemyTransform)
    {
        canMove = false;
        originalGravityScale = rb.gravityScale;
        rb.velocity = Vector2.zero; // Zera a velocidade para um empurrão limpo.

        Vector2 knockbackDirection = (transform.position - enemyTransform.position).normalized;
        rb.AddForce(knockbackDirection * knockbackForce, ForceMode2D.Impulse);

        if (isGrounded)
        {
            // Se estiver no chão, espera por um tempo fixo.
            yield return new WaitForSeconds(0.8f);
        }
        else
        {
            // Se estiver no ar, aplica gravidade extra e espera até aterrar.
            rb.gravityScale = knockbackAirGravityScale;
            yield return new WaitForFixedUpdate();
            while (!isGrounded)
            {
                yield return new WaitForFixedUpdate();
            }
        }

        rb.gravityScale = originalGravityScale; // Restaura a gravidade.
        canMove = true; // Devolve o controlo.
    }

    // Corrotina que lida com a morte.
    private IEnumerator Die()
    {
        isDead = true;
        //anim.SetTrigger("die"); 
        this.enabled = false; // Desativa o script para parar todos os Updates.
        rb.velocity = Vector2.zero;

        StartCoroutine(DamageFlashCoroutine());
        // Espera pela animação de morte.
        yield return new WaitForSeconds(1f);

        // Reinicia a cena.
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }


    // Corrotina para o efeito de piscar.
    private IEnumerator DamageFlashCoroutine()
    {
        for (int i = 0; i < 3; i++)
        {
            spriteRenderer.color = Color.red;
            yield return new WaitForSeconds(0.1f);
            spriteRenderer.color = Color.white;
            yield return new WaitForSeconds(0.1f);
        }
    }

    // Método chamado pelo evento de input "Attack".
    private void Attack(InputAction.CallbackContext context)
    {
        if (canMove && canAttack)
        {
            StartCoroutine(AttackCooldownCoroutine());
            slashAnimator.SetTrigger("attack");

            // Cria um círculo de ataque e deteta todos os inimigos dentro dele.
            Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayer);

            // Causa dano a cada inimigo atingido.
            foreach (Collider2D enemy in hitEnemies)
            {
                enemy.GetComponent<EnemyHealth>().TakeDamage(1);
            }
        }
    }

    // Corrotina para o cooldown do ataque.
    private IEnumerator AttackCooldownCoroutine()
    {
        canAttack = false;
        yield return new WaitForSeconds(attackCooldown);
        canAttack = true;
    }


    // Desenha a hitbox do ataque no Editor para fins de depuração.
    private void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }
}