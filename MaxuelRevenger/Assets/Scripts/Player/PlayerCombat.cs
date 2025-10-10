using System.Collections;
using UnityEngine;
using Unity.Netcode;

public class PlayerCombat : NetworkBehaviour
{
    [Header("Referências")]
    [SerializeField] private PlayerStatsRuntime playerStats;
    [SerializeField] public Animator slashAnimator;
    [SerializeField] private Transform attackPoint;
    [SerializeField] private LayerMask enemyLayer;

    private PlayerController player;
    private PlayerInput playerInput;
    private bool canAttack = true;

    [Header("Configurações de Combate")]
    [SerializeField] private float attackRange = 0.6f;
    [SerializeField] private float attackCooldown = 0.5f;

    private void Awake()
    {
        player = GetComponent<PlayerController>();
        playerInput = GetComponent<PlayerInput>();
    }

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
        if (player.motor.canAcceptInput.Value)
        {
            Debug.Log("[CLIENTE] Input de ataque recebido. Enviando pedido de ataque ao servidor...");
            AttackServerRpc();
        }
    }

    [ServerRpc]
    private void AttackServerRpc()
    {
        Debug.Log("[SERVIDOR] AttackServerRpc recebido.");

        if (!canAttack)
        {
            Debug.LogWarning("[SERVIDOR] Pedido de ataque ignorado: Cooldown ativo.");
            return;
        }
        Debug.Log("[SERVIDOR] Cooldown verificado. Prosseguindo com o ataque.");

        StartCoroutine(AttackCooldownCoroutine());
        TriggerAttackFeedbackClientRpc();

        Debug.Log($"[SERVIDOR] Verificando inimigos no ponto de ataque. Posição: {attackPoint.position}, Raio: {attackRange}, Layer: {LayerMask.LayerToName(Mathf.RoundToInt(Mathf.Log(enemyLayer.value, 2)))}");
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayer);

        Debug.Log($"[SERVIDOR] <color=cyan>{hitEnemies.Length} inimigos encontrados na área de ataque.</color>");

        foreach (Collider2D enemy in hitEnemies)
        {
            Debug.Log($"[SERVIDOR] Tentando aplicar dano em '{enemy.name}'.");
            var enemyHealth = enemy.GetComponent<EnemyHealth>();
            if (enemyHealth != null)
            {
                Debug.Log($"[SERVIDOR] <color=green>Componente EnemyHealth encontrado em '{enemy.name}'. Chamando TakeDamage.</color>");
                ulong attackerId = OwnerClientId;
                enemyHealth.TakeDamage(playerStats.attack.Value, attackerId);
            }
            else
            {
                Debug.LogError($"[SERVIDOR] ERRO: O objeto '{enemy.name}' na layer de inimigos não tem o componente EnemyHealth!");
            }
        }
    }

    [ClientRpc]
    private void TriggerAttackFeedbackClientRpc()
    {
        player.playerAnimator.TriggerAttackAnimation();
    }

    private IEnumerator AttackCooldownCoroutine()
    {
        canAttack = false;
        yield return new WaitForSeconds(attackCooldown);
        canAttack = true;
    }
}