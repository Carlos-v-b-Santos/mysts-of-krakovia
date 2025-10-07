using UnityEngine;

public class PlayerAnimator : MonoBehaviour
{
    private PlayerController player;
    private Animator anim;
    private Rigidbody2D rb;

    private readonly int isJumpingHash = Animator.StringToHash("isJumping");
    private readonly int speedHash = Animator.StringToHash("speed");
    private readonly int velocityYHash = Animator.StringToHash("velocityY");
    private readonly int jumpTriggerHash = Animator.StringToHash("jump");
    private readonly int attackTriggerHash = Animator.StringToHash("attack");
    private readonly int dieTriggerHash = Animator.StringToHash("die");

    private void Awake()
    {
        player = GetComponent<PlayerController>();
        anim = player.anim;
        rb = player.rb;
    }

    private void OnEnable()
    {
        // Ouve os eventos para disparar animações de ação única
        GameEventsManager.Instance.inputEvents.OnJumpPressed += TriggerJumpAnimation;
        GameEventsManager.Instance.inputEvents.OnAttackPressed += TriggerAttackAnimation;
    }

    private void OnDisable()
    {
        GameEventsManager.Instance.inputEvents.OnJumpPressed -= TriggerJumpAnimation;
        GameEventsManager.Instance.inputEvents.OnAttackPressed -= TriggerAttackAnimation;
    }

    void Update()
    {
        // Atualiza animações baseadas no estado (movimento, pulo, etc.)
        anim.SetBool(isJumpingHash, !player.motor.IsGrounded);
        anim.SetFloat(speedHash, Mathf.Abs(rb.velocity.x)); // Usa a velocidade real do Rigidbody
        anim.SetFloat(velocityYHash, rb.velocity.y);
    }

    // Métodos públicos para serem chamados por eventos ou outros scripts
    public void TriggerJumpAnimation()
    {
        if (player.motor.IsGrounded) // Só dispara a animação se estiver no chão
            anim.SetTrigger(jumpTriggerHash);
    }

    public void TriggerAttackAnimation()
    {
        if (player.motor.CanMove)
        {
            anim.SetTrigger(attackTriggerHash);
            player.combat.slashAnimator.SetTrigger("attack");
        }
    }

    public void TriggerDieAnimation() => anim.SetTrigger(dieTriggerHash);
}