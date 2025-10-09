using UnityEngine;
using Unity.Netcode;
using System.Collections;

public class EnemyHealth : NetworkBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private int maxHealth = 3;

    public NetworkVariable<int> currentHealth = new NetworkVariable<int>(
        readPerm: NetworkVariableReadPermission.Everyone,
        writePerm: NetworkVariableWritePermission.Server
    );

    private SpriteRenderer spriteRenderer;
    private EnemyPatrol patrolScript;
    private Animator anim;

    public override void OnNetworkSpawn()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        patrolScript = GetComponent<EnemyPatrol>();
        anim = GetComponent<Animator>();

        if (IsServer)
        {
            currentHealth.Value = maxHealth;
        }
    }

    // --- MUDANÇA PRINCIPAL AQUI ---
    // O método agora aceita o ID do atacante como um segundo parâmetro.
    public void TakeDamage(int damageAmount, ulong attackerId)
    {
        if (!IsServer) return;

        if (currentHealth.Value <= 0) return;

        // Log para depuração, mostrando quem atacou.
        Debug.Log($"Inimigo sofreu dano de {damageAmount} do jogador com ID: {attackerId}");

        currentHealth.Value -= damageAmount;

        if (currentHealth.Value <= 0)
        {
            DieClientRpc();
        }
        else
        {
            HurtClientRpc();
        }
    }

    [ClientRpc]
    private void HurtClientRpc()
    {
        StartCoroutine(DamageFlashCoroutine());
    }

    [ClientRpc]
    private void DieClientRpc()
    {
        if (patrolScript != null) patrolScript.enabled = false;
        GetComponent<Collider2D>().enabled = false;
        GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Static;

        if (IsServer)
        {
            StartCoroutine(DestroyAfterAnimation());
        }
    }

    private IEnumerator DamageFlashCoroutine()
    {
        spriteRenderer.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        spriteRenderer.color = Color.white;
    }

    private IEnumerator DestroyAfterAnimation()
    {
        yield return new WaitForSeconds(1.5f);
        GetComponent<NetworkObject>().Despawn();
    }
}