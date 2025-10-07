using UnityEngine;
using Unity.Netcode;
using System.Collections;

public class EnemyHealth : NetworkBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private int maxHealth = 3;

    // A vida atual é uma NetworkVariable, para que todos os clientes a vejam a diminuir.
    // Apenas o servidor pode alterar o seu valor.
    public NetworkVariable<int> currentHealth = new NetworkVariable<int>(
        readPerm: NetworkVariableReadPermission.Everyone,
        writePerm: NetworkVariableWritePermission.Server
    );

    // Referências para outros componentes
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

    // Este é o método público que o servidor chama para aplicar dano.
    public void TakeDamage(int damageAmount)
    {
        // Cláusula de guarda para garantir que a lógica de dano só corre no servidor.
        if (!IsServer) return;

        // Se já estiver morto, não faça nada.
        if (currentHealth.Value <= 0) return;

        currentHealth.Value -= damageAmount;

        if (currentHealth.Value <= 0)
        {
            // Se morreu, envia um comando para todos os clientes para executarem a sequência de morte.
            DieClientRpc();
        }
        else
        {
            // Se levou dano, envia um comando para todos os clientes para mostrarem o feedback.
            HurtClientRpc();
        }
    }

    // Um ClientRpc é um comando que o servidor envia para ser executado em TODOS os clientes.
    [ClientRpc]
    private void HurtClientRpc()
    {
        // Cada cliente executa o seu próprio feedback visual.
        StartCoroutine(DamageFlashCoroutine());
        // Se tivéssemos knockback no inimigo, a lógica começaria aqui.
        // anim.SetTrigger("hurt");
    }

    [ClientRpc]
    private void DieClientRpc()
    {
        // Todos os clientes veem o inimigo a morrer.
        if (patrolScript != null) patrolScript.enabled = false;
        GetComponent<Collider2D>().enabled = false;
        GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Static;

        // anim.SetTrigger("die");

        // Apenas o servidor irá destruir o objeto após a animação.
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
        yield return new WaitForSeconds(1.5f); // Duração da animação de morte
        GetComponent<NetworkObject>().Despawn(); // Despawn em vez de Destroy
    }
}