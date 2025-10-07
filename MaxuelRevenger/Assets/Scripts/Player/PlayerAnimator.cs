using UnityEngine;

public class PlayerAnimator : MonoBehaviour
{
    [Header("Referências")]
    [SerializeField] private PlayerController player; // Referência ao Hub do Player
    [SerializeField] private Animator anim;
    [SerializeField] private Rigidbody2D rb;

    // Usar Animator.StringToHash é uma ótima otimização para evitar o uso de strings em Update.
    private readonly int isJumpingHash = Animator.StringToHash("isJumping");
    private readonly int speedHash = Animator.StringToHash("speed");
    private readonly int velocityYHash = Animator.StringToHash("velocityY");
    private readonly int jumpTriggerHash = Animator.StringToHash("jump");
    private readonly int attackTriggerHash = Animator.StringToHash("attack");
    private readonly int dieTriggerHash = Animator.StringToHash("die");
    private readonly int hurtTriggerHash = Animator.StringToHash("hurt"); // Adicione um parâmetro "hurt" ao seu Animator se ainda não o fez

    // Awake é usado para obter as referências via Hub.
    private void Awake()
    {
        // Se as referências não forem definidas no Inspector, pegue-as através do Hub.
        if (player == null) player = GetComponent<PlayerController>();
        if (anim == null) anim = player.anim;
        if (rb == null) rb = player.rb;
    }

    // OnEnable é onde nos inscrevemos para "ouvir" os outros especialistas.
    private void OnEnable()
    {
        // Ouve os eventos de input para disparar animações de ação única.
        player.input.OnJumpPressed += TriggerJumpAnimation;
        player.input.OnAttackPressed += TriggerAttackAnimation;

        // Ouve os eventos de vida para as reações de dano e morte.
        player.health.OnHurt += TriggerHurtAnimation;
        player.health.OnDie += TriggerDieAnimation;
    }

    // Limpa as inscrições quando o objeto é desativado.
    private void OnDisable()
    {
        player.input.OnJumpPressed -= TriggerJumpAnimation;
        player.input.OnAttackPressed -= TriggerAttackAnimation;

        player.health.OnHurt -= TriggerHurtAnimation;
        player.health.OnDie -= TriggerDieAnimation;
    }

    // Update é usado para parâmetros que mudam constantemente (polling).
    private void Update()
    {
        // O componente Animator da Unity sincroniza parâmetros básicos (Float, Bool, Trigger)
        // automaticamente quando está num NetworkObject. Esta lógica pode correr em todas as instâncias.

        // Atualiza as animações com base no estado físico do PlayerMotor e Rigidbody2D.
        anim.SetBool(isJumpingHash, !player.motor.isGrounded);
        anim.SetFloat(speedHash, Mathf.Abs(rb.velocity.x));
        anim.SetFloat(velocityYHash, rb.velocity.y);
    }

    // --- MÉTODOS CHAMADOS PELOS EVENTOS ---

    private void TriggerJumpAnimation()
    {
        anim.SetTrigger(jumpTriggerHash);
    }

    public void TriggerAttackAnimation()
    {
        anim.SetTrigger(attackTriggerHash);
        // A animação do "slash" visual também é uma responsabilidade do Animator.
        if (player.combat.slashAnimator != null)
        {
            player.combat.slashAnimator.SetTrigger("attack");
        }
    }

    // O parâmetro 'enemyTransform' é enviado pelo evento, mas não precisamos dele aqui, apenas da notificação.
    public void TriggerHurtAnimation(Transform enemyTransform)
    {
        anim.SetTrigger(hurtTriggerHash);
    }

    public void TriggerDieAnimation(Transform enemyTransform)
    {
        anim.SetTrigger(dieTriggerHash);
    }
}