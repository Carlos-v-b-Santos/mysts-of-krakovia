using System.Collections;
using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    private PlayerController player;
    private RangeWeapon weapon; // Referência para a arma de longo alcance

    [Header("Attack Settings")]
    [SerializeField] public Animator slashAnimator;
    [SerializeField] public Transform attackPoint;
    [SerializeField] public Transform attackPivot;
    [SerializeField] private LayerMask enemyLayer;

    private bool canAttack = true;
    private bool isFiring = false;

    private void Awake()
    {
        player = GetComponent<PlayerController>();
        weapon = GetComponentInChildren<RangeWeapon>(); // Pega a arma no objeto filho
    }

    private void OnEnable()
    {
        GameEventsManager.Instance.inputEvents.OnAttackPressed += Attack;
        GameEventsManager.Instance.inputEvents.OnShootPressed += OnFireStarted;
        GameEventsManager.Instance.inputEvents.OnShootReleased += OnFireCanceled;
    }

    private void OnDisable()
    {
        GameEventsManager.Instance.inputEvents.OnAttackPressed -= Attack;
        GameEventsManager.Instance.inputEvents.OnShootPressed -= OnFireStarted;
        GameEventsManager.Instance.inputEvents.OnShootReleased -= OnFireCanceled;
    }

    private void Update()
    {
        // Se o botão de atirar estiver pressionado, continua atirando
        if (isFiring && weapon != null)
        {
            weapon.Disparar(); // Supondo que a arma tem seu próprio cooldown interno
        }
    }

    private void Attack()
    {
        if (player.motor.CanMove && canAttack)
        {
            StartCoroutine(AttackCooldownCoroutine());

            // A animação agora é chamada pelo PlayerAnimator
            // slashAnimator.SetTrigger("attack");

            Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, player.stats.attackRange, enemyLayer);

            foreach (Collider2D enemy in hitEnemies)
            {
                DamageManager.Instance.ApplyDamage(enemy.gameObject, 1, -1); // Usando seu DamageManager
            }
        }
    }

    private IEnumerator AttackCooldownCoroutine()
    {
        canAttack = false;
        yield return new WaitForSeconds(player.stats.attackCooldown);
        canAttack = true;
    }

    private void OnFireStarted() => isFiring = true;
    private void OnFireCanceled() => isFiring = false;

    private void OnDrawGizmosSelected()
    {
        if (attackPoint == null || player == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, player.stats.attackRange);
    }
}