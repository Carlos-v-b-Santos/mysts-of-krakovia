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
    private EnemyController enemyController;
    private Animator anim;

    // Hashes para otimização
    private readonly int hurtBoolHash = Animator.StringToHash("hurt");
    private readonly int dieBoolHash = Animator.StringToHash("die");


    public override void OnNetworkSpawn()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        enemyController = GetComponent<EnemyController>();
        anim = GetComponent<Animator>();

        if (IsServer)
        {
            currentHealth.Value = maxHealth;
        }
    }

    public void TakeDamage(int damageAmount, ulong attackerId)
    {
        if (!IsServer) return;
        if (currentHealth.Value <= 0) return;

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
        // Inicia o feedback visual de flash E a animação de dano.
        StartCoroutine(DamageFlashCoroutine());
        StartCoroutine(HurtAnimationCoroutine());
    }

    [ClientRpc]
    private void DieClientRpc()
    {
        if (enemyController != null) enemyController.enabled = false;
        GetComponent<Collider2D>().enabled = false;
        GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Static;

        // Ativa o estado de morte no Animator.
        anim.SetBool(dieBoolHash, true);

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

    // Coroutine para imitar um Trigger com o Bool "hurt".
    private IEnumerator HurtAnimationCoroutine()
    {
        anim.SetBool(hurtBoolHash, true);
        yield return null; // Espera um frame
        anim.SetBool(hurtBoolHash, false);
    }

    private IEnumerator DestroyAfterAnimation()
    {
        yield return new WaitForSeconds(1.5f);
        GetComponent<NetworkObject>().Despawn();
    }
}