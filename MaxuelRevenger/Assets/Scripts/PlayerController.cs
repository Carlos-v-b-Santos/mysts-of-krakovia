using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;


public class PlayerController : MonoBehaviour
{
    // Variáveis de configuração expostas no Inspector

    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 8f;
    [SerializeField] private float jumpForce = 15f;

    [Header("Ground Check Settings")]
    [SerializeField] private Transform groundCheckPoint;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float groundCheckRadius = 0.2f;


    [Header("Life System")] 
    [SerializeField] private int maxHealth = 3;
    private int currentHealth;
    [SerializeField] private float knockbackForce = 5f;
   
    [SerializeField] private float knockbackAirGravityScale = 4f; //Gravidade extra no ar
    private float originalGravityScale; // Variável para guardar a gravidade original

    [Header("Attack Settings")]
    [SerializeField] private Animator slashAnimator;
    [SerializeField] private Transform attackPoint;
    [SerializeField] private float attackRange = 2.3f;
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private float attackCooldown = 0.5f; //  Tempo de espera em segundos
    [SerializeField] private Transform attackPivot;
    private bool canAttack = true; //  Controla se o jogador pode atacar



    // Referências a componentes e classes
    private Rigidbody2D rb;
    private PlayerControls playerControls;
    // private Animator anim;
    private SpriteRenderer spriteRenderer;
    private Vector2 moveInput;
    private bool isGrounded;
    private bool canMove = true;
    private bool isDead = false;
    private bool isInvincible = false;
    



    // Awake é chamado quando a instância do script é carregada
    private void Awake()
    {
        // Obtém a referência para o componente Rigidbody2D no mesmo GameObject
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        // - - - - - - Referência Do Animator - - - - - - - 
        //anim = GetComponent<Animator>();

        // Instancia a classe de controles gerada automaticamente
        playerControls = new PlayerControls();
       

        currentHealth = maxHealth;
    }

    // OnEnable é chamado quando o objeto se torna ativo
    private void OnEnable()
    {
        // Habilita o Action Map "Gameplay"
        playerControls.Gameplay.Enable();

        // Inscreve o método Jump no evento 'performed' da ação Jump.
        // Isso adota uma abordagem orientada a eventos, mais eficiente para ações discretas.
        playerControls.Gameplay.Jump.performed += Jump;
        playerControls.Gameplay.Attack.performed += Attack;
    }

    // OnDisable é chamado quando o objeto se torna inativo
    private void OnDisable()
    {
        // Desabilita o Action Map para evitar processamento desnecessário
        playerControls.Gameplay.Disable();

        // Desinscreve o método Jump para evitar memory leaks (vazamentos de memória)
        playerControls.Gameplay.Jump.performed -= Jump;
        playerControls.Gameplay.Attack.performed -= Attack;
    }

    // Update é chamado a cada frame
    private void Update()
    {
        // Só lê o input se o jogador puder se mover
        if (canMove)
        {
            moveInput.x = playerControls.Gameplay.Move.ReadValue<float>();
        }
        else
        {
            moveInput.x = 0; // Garante que o jogador para se não puder se mover
        }
        // --- LÓGICA DE INVERSÃO DO SPRITE ---
        if (moveInput.x > 0)
        {
            spriteRenderer.flipX = false; // Vira o sprite do jogador
            attackPivot.localScale = Vector3.one; // Restaura a escala do pivô (sem inversão)
        }
        else if (moveInput.x < 0)
        {
            spriteRenderer.flipX = true; // Vira o sprite do jogador
            attackPivot.localScale = new Vector3(-1, 1, 1); // Inverte a escala do pivô no eixo X
        }
        // ------------------------------------
        //anim.SetFloat("speed", Mathf.Abs(moveInput.x));
        //anim.SetFloat("velocityY", rb.velocity.y);
    }

    // FixedUpdate é chamado em um intervalo de tempo fixo, ideal para cálculos de física
    private void FixedUpdate()
    {
        CheckIfGrounded();

        // Só aplica o movimento se o jogador puder se mover
        if (canMove)
        {
            rb.velocity = new Vector2(moveInput.x * moveSpeed, rb.velocity.y);
        }

        //anim.SetBool("isJumping", !isGrounded);
    }


    // Método chamado pelo evento da ação Jump
    private void Jump(InputAction.CallbackContext context)
    {
        // Só permite o pulo se o personagem estiver no chão
        if (isGrounded)
        {
            //anim.SetTrigger("jump");

            // Aplica uma força vertical instantânea (impulso)
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        }
    }

    // Método para verificar se o personagem está tocando o chão
    private void CheckIfGrounded()
    {
        // Cria um círculo invisível na posição do groundCheckPoint
        // e verifica se ele colide com qualquer objeto na camada "groundLayer"
        isGrounded = Physics2D.OverlapCircle(groundCheckPoint.position, groundCheckRadius, groundLayer);
    }

    public void TakeDamage(int damageAmount, Transform enemyTransform)
    {
        if (isInvincible || isDead || !canMove) return;

        currentHealth -= damageAmount;
        Debug.Log("Player tomou dano! Vida atual: " + currentHealth);

        if (currentHealth <= 0)
        {
            StartCoroutine(Die());
        }
        else
        {
            // Se o dano não for fatal, execute a animação e o knockback.
           
            StartCoroutine(DamageFlashCoroutine());
            StartCoroutine(KnockbackCoroutine(enemyTransform));
        }
    }


    private IEnumerator KnockbackCoroutine(Transform enemyTransform)
    {
        canMove = false; // Desativa o controle do jogador
        originalGravityScale = rb.gravityScale; // Guarda a gravidade original

        // ZERA a velocidade atual para cancelar qualquer movimento existente (como o pulo)
        rb.velocity = Vector2.zero;

        // Calcula a direção e aplica o empurrão
        Vector2 knockbackDirection = (transform.position - enemyTransform.position).normalized;
        rb.velocity = Vector2.zero; // Zera a velocidade atual para que o empurrão seja limpo
        rb.AddForce(knockbackDirection * knockbackForce, ForceMode2D.Impulse);

        // Verifica se o jogador estava no chão QUANDO sofreu o dano
        if (isGrounded)
        {
            yield return new WaitForSeconds(0.8f); // Usamos um tempo fixo para o chão
        }
        else
        {
            // Se estiver no ar, aumente a gravidade para uma queda mais rápida
            rb.gravityScale = knockbackAirGravityScale;

            yield return new WaitForFixedUpdate();
            while (!isGrounded)
            {
                yield return new WaitForFixedUpdate();
            }
        }

        // Restaura a gravidade original ANTES de devolver o controlo
        rb.gravityScale = originalGravityScale;
        canMove = true; 
    }

    private IEnumerator Die()
    {
        isDead = true; // Marca o jogador como morto

        // - - - - - Ativa animação de morte - - - - - -
        //anim.SetTrigger("die"); 

        this.enabled = false; // Desativa o script para parar o movimento
        rb.velocity = Vector2.zero; // Para o jogador imediatamente

        StartCoroutine(DamageFlashCoroutine());
        // ESPERE a animação de morte terminar.
        // Ajuste este valor para a duração da sua animação de morte em segundos.
        yield return new WaitForSeconds(1f);

        // Reinicia a cena atual
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }



    private IEnumerator DamageFlashCoroutine()
    {
        // Pisca 3 vezes
        for (int i = 0; i < 3; i++)
        {
            spriteRenderer.color = Color.red;
            yield return new WaitForSeconds(0.1f);
            spriteRenderer.color = Color.white;
            yield return new WaitForSeconds(0.1f);
        }
    }

    private void Attack(InputAction.CallbackContext context)
    {
        // Verifica se o jogador pode se mover E se o ataque não está em cooldown
        if (canMove && canAttack)
        {
            // Inicia a corrotina de cooldown
            StartCoroutine(AttackCooldownCoroutine());

            // Toca a animação visual do ataque
            slashAnimator.SetTrigger("attack");

            // Cria um círculo invisível para detetar inimigos
            Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayer);

            // Passa por cada inimigo atingido e aplica dano
            foreach (Collider2D enemy in hitEnemies)
            {
                enemy.GetComponent<EnemyHealth>().TakeDamage(1);
            }
        }
    }

    private IEnumerator AttackCooldownCoroutine()
    {
        canAttack = false; // Impede o jogador de atacar novamente
        yield return new WaitForSeconds(attackCooldown); // Espera pelo tempo de cooldown
        canAttack = true; // Permite que o jogador ataque novamente
    }

    // Este método especial da Unity desenha coisas na janela da Scene
    private void OnDrawGizmosSelected()
    {
        // Garante que temos uma referência ao attackPoint para não dar erro
        if (attackPoint == null)
        {
            return;
        }

        // Define a cor do nosso gizmo
        Gizmos.color = Color.red;
        // Desenha uma esfera de arame na posição e com o raio do nosso ataque
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }

 

}