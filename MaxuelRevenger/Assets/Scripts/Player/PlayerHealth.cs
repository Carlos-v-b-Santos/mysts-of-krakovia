using System.Collections;
using UnityEngine;
using Unity.Netcode;
using System;

public class PlayerHealth : NetworkBehaviour
{
    [Header("Referências")]
    [SerializeField] private PlayerController player;
    [SerializeField] private PlayerStatsRuntime playerStats;
    private PlayerMotor playerMotor;

    public event Action<Transform> OnHurt;
    public event Action<Transform> OnDie;

    private bool isDead = false;

    private void Awake()
    {
        playerMotor = GetComponent<PlayerMotor>();
    }

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            playerStats.current_health.OnValueChanged += OnHealthChanged;
        }
    }

    public override void OnNetworkDespawn()
    {
        if (IsOwner)
        {
            playerStats.current_health.OnValueChanged -= OnHealthChanged;
        }
    }

    // Este método é chamado AUTOMATICAMENTE no cliente quando a vida muda no servidor.
    private void OnHealthChanged(int previousValue, int newValue)
    {
        // ++ NOVO DEBUG LOG ++
        // Adicionamos um LogWarning para se destacar em amarelo no console.
        Debug.LogWarning($"VIDA DO JOGADOR MUDOU! Vida anterior: {previousValue}, Vida atual: {newValue}");

        if (newValue < previousValue)
        {
            OnHurt?.Invoke(null);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void TakeDamageServerRpc(int damageAmount, ulong attackerId)
    {
        if (isDead) return;

        playerStats.current_health.Value -= damageAmount;

        Transform attackerTransform = null;
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(attackerId, out NetworkObject attackerObject))
        {
            attackerTransform = attackerObject.transform;
        }

        if (attackerTransform != null)
        {
            playerMotor.ApplyKnockback(attackerTransform);
        }

        if (playerStats.current_health.Value <= 0)
        {
            isDead = true;
            DieClientRpc(attackerId);
        }
    }

    [ClientRpc]
    private void DieClientRpc(ulong attackerId)
    {
        isDead = true;

        Transform attackerTransform = null;
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(attackerId, out NetworkObject attackerObject))
        {
            attackerTransform = attackerObject.transform;
        }

        OnDie?.Invoke(attackerTransform);
        player.playerAnimator.TriggerDieAnimation();

        if (IsOwner)
        {
            StartCoroutine(DieSequence());
        }
    }

    private IEnumerator DieSequence()
    {
        yield return new WaitForSeconds(2.0f);
        // Lógica de fim de jogo...
    }
}