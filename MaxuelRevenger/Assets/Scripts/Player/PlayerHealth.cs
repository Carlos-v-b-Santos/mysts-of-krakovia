using System.Collections;
using UnityEngine;
using Unity.Netcode; // Importe o Netcode
using UnityEngine.SceneManagement;
using System; // Importe para usar Actions (eventos)

public class PlayerHealth : NetworkBehaviour // Mude para NetworkBehaviour
{
    [Header("Refer�ncias")]
    [SerializeField] private PlayerController player;
    [SerializeField] private PlayerStatsRuntime playerStats;
    private PlayerMotor playerMotor;

    // --- EVENTOS P�BLICOS ---
    // Outros componentes (Animator, Motor) ir�o "ouvir" estes an�ncios.
    public event Action<Transform> OnHurt;
    public event Action<Transform> OnDie;

    private bool isDead = false;

    private void Awake()
    {
        playerMotor = GetComponent<PlayerMotor>();
    }

    // OnNetworkSpawn � onde a l�gica de rede come�a.
    public override void OnNetworkSpawn()
    {
        // Apenas o jogador local precisa de se inscrever para ouvir as suas pr�prias mudan�as de vida.
        if (IsOwner)
        {
            // Inscreve o m�todo OnHealthChanged para ser chamado sempre que a NetworkVariable de vida mudar no servidor.
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

    // Este m�todo � chamado AUTOMATICAMENTE em todos os clientes quando a vida do jogador muda no servidor.
    private void OnHealthChanged(int previousValue, int newValue)
    {
        // Se a vida diminuiu, significa que sofremos dano.
        if (newValue < previousValue)
        {
            // Como a l�gica de dano j� foi confirmada pelo servidor, agora apenas
            // anunciamos o evento de "Hurt" para os outros componentes locais reagirem.
            OnHurt?.Invoke(null); // Passamos null por enquanto, pois a origem do dano n�o � sincronizada neste exemplo.
        }
    }

    // Este m�todo � um PEDIDO que um atacante (como um inimigo ou outro jogador) faz.
    // Ele corre apenas no SERVIDOR.
    [ServerRpc(RequireOwnership = false)]
    public void TakeDamageServerRpc(int damageAmount, ulong attackerId)
    {
        if (isDead) return;

        // O SERVIDOR reduz o valor da vida.
        playerStats.current_health.Value -= damageAmount;

        // --- MUDAN�A PRINCIPAL: INICIAR O KNOCKBACK AQUI ---
        // 1. Encontra o objeto do atacante que est� na rede.
        Transform attackerTransform = null;
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(attackerId, out NetworkObject attackerObject))
        {
            attackerTransform = attackerObject.transform;
        }

        // 2. Se o atacante foi encontrado, chama o m�todo de knockback no PlayerMotor.
        if (attackerTransform != null)
        {
            playerMotor.ApplyKnockback(attackerTransform);
        }
        

        if (playerStats.current_health.Value <= 0)
        {
            isDead = true;
            // O servidor determina a morte e envia um comando para todos os clientes.
            DieClientRpc(attackerId);
        }
    }

    // Este m�todo � um COMANDO que o servidor envia para TODOS OS CLIENTES.
    [ClientRpc]
    private void DieClientRpc(ulong attackerId)
    {
        // Todos os clientes executam a l�gica visual da morte.
        isDead = true;

        // Encontra o objeto do atacante para o knockback
        Transform attackerTransform = null;
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(attackerId, out NetworkObject attackerObject))
        {
            attackerTransform = attackerObject.transform;
        }

        // Anuncia o evento de morte para os outros componentes locais reagirem.
        OnDie?.Invoke(attackerTransform);
        player.playerAnimator.TriggerDieAnimation();

        // Apenas o jogador local ir� lidar com a l�gica de fim de jogo (ex: reiniciar a cena).
        if (IsOwner)
        {
            StartCoroutine(DieSequence());
        }
    }

    // A corrotina de morte corre apenas para o jogador local.
    private IEnumerator DieSequence()
    {
        // Espera pela anima��o de morte e knockback.
        yield return new WaitForSeconds(2.0f);
        // L�gica de fim de jogo, como voltar ao menu principal.
        // SceneManager.LoadScene("MainMenu"); 
    }
}