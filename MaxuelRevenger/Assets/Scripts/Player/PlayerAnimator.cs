using UnityEngine;

public class PlayerAnimator : MonoBehaviour
{
    [Header("Refer�ncias")]
    [SerializeField] private PlayerController player; // Refer�ncia ao Hub do Player
    [SerializeField] private Animator anim;
    [SerializeField] private Rigidbody2D rb;

    // Usar Animator.StringToHash � uma �tima otimiza��o para evitar o uso de strings em Update.
    private readonly int isJumpingHash = Animator.StringToHash("isJumping");
    private readonly int speedHash = Animator.StringToHash("speed");
    private readonly int velocityYHash = Animator.StringToHash("velocityY");
    private readonly int jumpTriggerHash = Animator.StringToHash("jump");
    private readonly int attackTriggerHash = Animator.StringToHash("attack");
    private readonly int dieTriggerHash = Animator.StringToHash("die");
    private readonly int hurtTriggerHash = Animator.StringToHash("hurt"); // Adicione um par�metro "hurt" ao seu Animator se ainda n�o o fez

    // Awake � usado para obter as refer�ncias via Hub.
    private void Awake()
    {
        // Se as refer�ncias n�o forem definidas no Inspector, pegue-as atrav�s do Hub.
        if (player == null) player = GetComponent<PlayerController>();
        if (anim == null) anim = player.anim;
        if (rb == null) rb = player.rb;
    }

    // OnEnable � onde nos inscrevemos para "ouvir" os outros especialistas.
    private void OnEnable()
    {
        // Ouve os eventos de input para disparar anima��es de a��o �nica.
        player.input.OnJumpPressed += TriggerJumpAnimation;
        player.input.OnAttackPressed += TriggerAttackAnimation;

        // Ouve os eventos de vida para as rea��es de dano e morte.
        player.health.OnHurt += TriggerHurtAnimation;
        player.health.OnDie += TriggerDieAnimation;
    }

    // Limpa as inscri��es quando o objeto � desativado.
    private void OnDisable()
    {
        player.input.OnJumpPressed -= TriggerJumpAnimation;
        player.input.OnAttackPressed -= TriggerAttackAnimation;

        player.health.OnHurt -= TriggerHurtAnimation;
        player.health.OnDie -= TriggerDieAnimation;
    }

    // Update � usado para par�metros que mudam constantemente (polling).
    private void Update()
    {
        // O componente Animator da Unity sincroniza par�metros b�sicos (Float, Bool, Trigger)
        // automaticamente quando est� num NetworkObject. Esta l�gica pode correr em todas as inst�ncias.

        // Atualiza as anima��es com base no estado f�sico do PlayerMotor e Rigidbody2D.
        anim.SetBool(isJumpingHash, !player.motor.isGrounded);
        anim.SetFloat(speedHash, Mathf.Abs(rb.velocity.x));
        anim.SetFloat(velocityYHash, rb.velocity.y);
    }

    // --- M�TODOS CHAMADOS PELOS EVENTOS ---

    private void TriggerJumpAnimation()
    {
        anim.SetTrigger(jumpTriggerHash);
    }

    public void TriggerAttackAnimation()
    {
        anim.SetTrigger(attackTriggerHash);
        // A anima��o do "slash" visual tamb�m � uma responsabilidade do Animator.
        if (player.combat.slashAnimator != null)
        {
            player.combat.slashAnimator.SetTrigger("attack");
        }
    }

    // O par�metro 'enemyTransform' � enviado pelo evento, mas n�o precisamos dele aqui, apenas da notifica��o.
    public void TriggerHurtAnimation(Transform enemyTransform)
    {
        anim.SetTrigger(hurtTriggerHash);
    }

    public void TriggerDieAnimation(Transform enemyTransform)
    {
        anim.SetTrigger(dieTriggerHash);
    }
}