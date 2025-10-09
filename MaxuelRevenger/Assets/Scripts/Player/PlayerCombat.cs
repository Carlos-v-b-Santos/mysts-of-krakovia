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
    [ClientRpc]
    private void TriggerAttackFeedbackClientRpc()
    {
        // Este comando do servidor diz a todos os clientes para mostrarem o efeito visual do ataque.
        player.playerAnimator.TriggerAttackAnimation();
    }
    private void HandleAttack()
    {
        // O cliente agora só verifica se tem controlo do personagem (não está em knockback).
        // Ele NÃO verifica mais o cooldown do ataque.
        if (player.motor.canAcceptInput.Value)
        {
            // A única responsabilidade do cliente é PEDIR um ataque ao servidor.
            AttackServerRpc();
        }
    }

    [ServerRpc]
    private void AttackServerRpc()
    {
        // --- ESTE CÓDIGO AGORA EXECUTA APENAS NO SERVIDOR ---

        // 1. O SERVIDOR verifica o cooldown.
        if (!canAttack) return; // Se estiver em cooldown, ignora o pedido.

        // 2. Se o ataque for válido, o SERVIDOR inicia o seu próprio cooldown.
        StartCoroutine(AttackCooldownCoroutine());

        // 3. O SERVIDOR comanda a todos os clientes para tocarem a animação.
        TriggerAttackFeedbackClientRpc();

        // 4. O SERVIDOR executa a lógica de detecção de dano.
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayer);

        foreach (Collider2D enemy in hitEnemies)
        {
            var enemyHealth = enemy.GetComponent<EnemyHealth>();
            if (enemyHealth != null)
            {
                // O NetworkObjectID do jogador que está a atacar.
                ulong attackerId = OwnerClientId;
                enemyHealth.TakeDamage(playerStats.attack.Value, attackerId);
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