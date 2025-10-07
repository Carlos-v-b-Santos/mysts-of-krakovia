using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using Unity.VisualScripting;
using System;


public class Temp : MonoBehaviour
{
    // --- VARI�VEIS DE CONFIGURA��O (AJUST�VEIS NO INSPECTOR) ---
    // [Header] cria um t�tulo no Inspector para organiza��o.
    // [SerializeField] exp�e uma vari�vel privada no Inspector.

    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 8f;      // Velocidade de movimento do jogador.
    [SerializeField] private float jumpForce = 6f;     // For�a do impulso do pulo.

    [Header("Ground Check Settings")]
    [SerializeField] private Transform groundCheckPoint;  // Ponto de refer�ncia nos "p�s" do jogador para detetar o ch�o.
    [SerializeField] private LayerMask groundLayer;       // Define o que � considerado "ch�o" para o nosso sensor.
    [SerializeField] private float groundCheckRadius = 0.2f; // Raio do c�rculo do sensor de ch�o.

    [Header("Life System")]
    [SerializeField] private int maxHealth = 3;           // Vida m�xima do jogador.
    [SerializeField] private float knockbackForce = 5f;     // For�a do empurr�o ao sofrer dano.
    [SerializeField] private float knockbackAirGravityScale = 4f; // Gravidade extra aplicada durante o knockback a�reo para uma queda mais r�pida.

    [Header("Attack Settings")]
    [SerializeField] private Animator slashAnimator;    // Refer�ncia ao Animator do efeito de corte.
    [SerializeField] private Transform attackPoint;     // Ponto de refer�ncia de onde o ataque se origina.
    [SerializeField] private float attackRange = 2.3f;    // Raio do c�rculo da hitbox do ataque.
    [SerializeField] private LayerMask enemyLayer;      // Define o que � considerado um "inimigo" para o nosso ataque.
    [SerializeField] private float attackCooldown = 0.5f; // Tempo de espera entre ataques.
    [SerializeField] private Transform attackPivot;     // Piv� que segura os objetos de ataque para virar com o jogador.

    // --- VARI�VEIS DE ESTADO INTERNAS (A "MEM�RIA" DO JOGADOR) ---
    private int currentHealth;              // Vida atual durante o jogo.
    private float originalGravityScale;     // Guarda a gravidade normal para a podermos restaurar ap�s o knockback.
    private bool canAttack = true;          // Controla se o jogador pode atacar.
    private Rigidbody2D rb;                 // Refer�ncia ao componente de f�sica.
    private PlayerControls playerControls;  // Refer�ncia ao nosso mapa de inputs.
    private Animator anim;                  // Refer�ncia ao componente Animator do jogador.
    private SpriteRenderer spriteRenderer;  // Refer�ncia ao componente que desenha o sprite.
    private Vector2 moveInput;              // Guarda a dire��o do input de movimento.
    private bool isGrounded;                // Est� a tocar no ch�o?
    private bool canMove = true;            // Pode mover-se?
    private bool isDead = false;            // Est� morto?
    private bool isInvincible = false;      // Est� invenc�vel? (Ainda n�o implementado, mas preparado)
    private bool isJumping;                 // Est� a pular? (Usado pelo Animator)

    private bool isFiring;

    PlayerInputManager inputManager;

    PlayerStatsRuntime playerStats;
    RangeWeapon weapon;

    // Awake corre uma vez, antes do Start. Ideal para inicializar refer�ncias.
    private void Awake()
    {
        // "Apanha" os componentes que est�o no mesmo GameObject.
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();

        // Cria e ativa o nosso mapa de inputs.
        //playerControls = new PlayerControls();
        inputManager = PlayerInputManager.GetInstance();
        inputManager.EnterGameplayMode();
        //playerControls.Gameplay.Enable();

        // Define a vida inicial.
        currentHealth = maxHealth;

        playerStats = GetComponent<PlayerStatsRuntime>();

        weapon = GetComponentInChildren<RangeWeapon>();

    }

    // OnEnable corre quando o objeto � ativado. Perfeito para "inscrever-se" em eventos.
    private void OnEnable()
    {
        GameEventsManager.Instance.inputEvents.OnJumpPressed += Jump;
        GameEventsManager.Instance.inputEvents.OnAttackPressed += Attack;
        GameEventsManager.Instance.inputEvents.OnShootPressed += OnFireStarted;
        GameEventsManager.Instance.inputEvents.OnShootReleased += OnFireCanceled;
        GameEventsManager.Instance.inputEvents.OnMovePressed += MovePressed;
    }

    // OnDisable corre quando o objeto � desativado. Limpamos as inscri��es para evitar erros.
    private void OnDisable()
    {
        GameEventsManager.Instance.inputEvents.OnJumpPressed -= Jump;
        GameEventsManager.Instance.inputEvents.OnAttackPressed -= Attack;
        GameEventsManager.Instance.inputEvents.OnShootPressed -= OnFireStarted;
        GameEventsManager.Instance.inputEvents.OnShootReleased -= OnFireCanceled;
        GameEventsManager.Instance.inputEvents.OnMovePressed -= MovePressed;
    }

    // FixedUpdate corre a um ritmo fixo, sincronizado com a f�sica.
    private void FixedUpdate()
    {
        // Apenas l� o input de movimento se o jogador tiver controlo.
        //if (canMove)
        //{
        //moveInput.x = playerControls.Gameplay.Move.ReadValue<float>();
        //}
        //else
        //{
        //    moveInput.x = 0;
        //}

        CheckIfGrounded();

        // Apenas aplica o movimento se o jogador tiver controlo.
        if (canMove)
        {
            // Aplicamos o movimento diretamente � velocidade do Rigidbody.
            rb.velocity = new Vector2(moveInput.x * moveSpeed, rb.velocity.y);
        }

        // L�gica para virar o sprite do jogador e o piv� do ataque.
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

        if (isFiring)
        {
            weapon.SetStats(playerStats.attack, playerStats.dexterity);
            ShootPressed();
        }

        // Comunica��o com o Animator (a ser ativada na aula).
        anim.SetBool("isJumping", !isGrounded);
        anim.SetFloat("speed", Mathf.Abs(moveInput.x));
        anim.SetFloat("velocityY", rb.velocity.y);
    }


    // M�todo chamado pelo evento de input "Jump".
    private void Jump()
    {
        // A condi��o impede o "pulo duplo".
        if (isGrounded)
        {
            anim.SetTrigger("jump");
            // Adiciona uma for�a vertical instant�nea.
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        }
    }

    // Verifica se o "sensor de p�s" est� a tocar em algo na layer "Ground".
    private void CheckIfGrounded()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheckPoint.position, groundCheckRadius, groundLayer);
    }

    // M�todo p�blico que pode ser chamado por outros scripts (como o do inimigo).
    public void TakeDamage(int damageAmount, Transform enemyTransform)
    {
        // Cl�usula de guarda: ignora o dano se o jogador j� estiver invenc�vel ou morto.
        if (isInvincible || isDead || !canMove) return;

        currentHealth -= damageAmount;
        Debug.Log("Player tomou dano! Vida atual: " + currentHealth);

        if (currentHealth <= 0)
        {
            // Se o dano for fatal, inicia a sequ�ncia de morte com knockback.
            StartCoroutine(FatalHitSequence(enemyTransform));
        }
        else
        {
            // Se n�o for fatal, inicia o flash e o knockback.
            StartCoroutine(DamageFlashCoroutine());
            StartCoroutine(KnockbackCoroutine(enemyTransform));
        }
    }

    // Corrotina que orquestra a sequ�ncia de knockback e depois a de morte.
    private IEnumerator FatalHitSequence(Transform enemyTransform)
    {
        StartCoroutine(DamageFlashCoroutine());
        // yield return espera que a corrotina de knockback termine completamente.
        yield return StartCoroutine(KnockbackCoroutine(enemyTransform));
        // S� depois de o knockback terminar, inicia a corrotina de morte.
        StartCoroutine(Die());
    }

    // Corrotina que lida com o knockback e a perda de controlo.
    private IEnumerator KnockbackCoroutine(Transform enemyTransform)
    {
        canMove = false;
        originalGravityScale = rb.gravityScale;
        rb.velocity = Vector2.zero; // Zera a velocidade para um empurr�o limpo.

        Vector2 knockbackDirection = (transform.position - enemyTransform.position).normalized;
        rb.AddForce(knockbackDirection * knockbackForce, ForceMode2D.Impulse);

        if (isGrounded)
        {
            // Se estiver no ch�o, espera por um tempo fixo.
            yield return new WaitForSeconds(0.8f);
        }
        else
        {
            // Se estiver no ar, aplica gravidade extra e espera at� aterrar.
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
        anim.SetTrigger("die");
        this.enabled = false; // Desativa o script para parar todos os Updates.
        rb.velocity = Vector2.zero;

        StartCoroutine(DamageFlashCoroutine());
        // Espera pela anima��o de morte.
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

    // M�todo chamado pelo evento de input "Attack".
    private void Attack()
    {
        if (canMove && canAttack)
        {
            StartCoroutine(AttackCooldownCoroutine());
            slashAnimator.SetTrigger("attack");

            // Cria um c�rculo de ataque e deteta todos os inimigos dentro dele.
            Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayer);

            // Causa dano a cada inimigo atingido.
            foreach (Collider2D enemy in hitEnemies)
            {
                //enemy.GetComponent<EnemyHealth>().TakeDamage(1);
                DamageManager.Instance.ApplyDamage(enemy.gameObject, 1, -1);
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


    // Desenha a hitbox do ataque no Editor para fins de depura��o.
    private void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }

    //private void RegisterInteractPressed(InputAction.CallbackContext context)
    //{
    //    if (context.performed)
    //    {
    //        InteractPressed = true;
    //        GameEventsManager.Instance.inputEvents.InteractPressed();
    //        InteractPressed = false;
    //    }
    //}

    //public bool GetInteractPressed()
    //{
    //    bool result = InteractPressed;
    //    InteractPressed = false;
    //    return result;
    //}

    //public void OnQuestLogToggle(InputAction.CallbackContext context)
    //{
    //    print("Quest Log Toggled");
    //    if (context.performed)
    //    {
    //        GameEventsManager.Instance.inputEvents.QuestLogTogglePressed();
    //    }
    //}

    public void ShootPressed()
    {

        if (weapon != null)
        {
            weapon.Disparar();
        }

    }

    public void OnFireStarted()
    {
        isFiring = true;
    }

    public void OnFireCanceled()
    {
        isFiring = false;
    }

    public void MovePressed(float direction)
    {
        moveInput.x = direction;
    }
}