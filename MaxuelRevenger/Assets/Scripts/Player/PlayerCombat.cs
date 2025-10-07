using System.Collections;
using UnityEngine;
using Unity.Netcode; // Importe o Netcode

public class PlayerCombat : NetworkBehaviour // Mude para NetworkBehaviour
{
    [Header("Referências")]
    [SerializeField] private PlayerStatsRuntime playerStats;
    [SerializeField] public Animator slashAnimator;
    [SerializeField] private Transform attackPoint;
    [SerializeField] private LayerMask enemyLayer;

    // Referência para o nosso script de input
    private PlayerController player;
    private PlayerInput playerInput;

    // Variáveis de estado
    private bool canAttack = true;


    [Header("Configurações de Combate")]
    [SerializeField] private float attackRange = 0.6f; // <-- A VARIÁVEL AGORA VIVE AQUI!
    [SerializeField] private float attackCooldown = 0.5f;

    private void Awake()
    {
        // Apanha a referência para o PlayerInput que está no mesmo GameObject.
        player = GetComponent<PlayerController>();
        playerInput = GetComponent<PlayerInput>();
    }

    // Apenas o jogador local deve "ouvir" os seus próprios inputs de ataque.
    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            playerInput.OnAttackPressed += HandleAttack;
        }
    }

    public override void OnNetworkDespawn()
    {
        if (IsOwner)
        {
            playerInput.OnAttackPressed -= HandleAttack;
        }
    }
    private void HandleAttack()
    {
        if (player.motor.CanMove && canAttack)
        {
            StartCoroutine(AttackCooldownCoroutine());
            player.playerAnimator.TriggerAttackAnimation();

            // Agora usa a variável 'attackRange' local deste script
            AttackServerRpc();
        }
    }

    [ServerRpc]
    private void AttackServerRpc()
    {
        // O servidor usa as variáveis locais do seu próprio componente PlayerCombat
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayer);

        foreach (Collider2D enemy in hitEnemies)
        {
            var enemyHealth = enemy.GetComponent<EnemyHealth>();
            if (enemyHealth != null)
            {
                // Usa o stat de ataque do PlayerStatsRuntime
                enemyHealth.TakeDamage(playerStats.attack.Value);
            }
        }
    }

    private IEnumerator AttackCooldownCoroutine()
    {
        canAttack = false;
        yield return new WaitForSeconds(attackCooldown);
        canAttack = true;
    }
}