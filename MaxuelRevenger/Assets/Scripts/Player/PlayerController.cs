using UnityEngine;
using Unity.Netcode;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(PlayerInput))]
[RequireComponent(typeof(PlayerMotor))]
[RequireComponent(typeof(PlayerHealth))]
[RequireComponent(typeof(PlayerCombat))]
[RequireComponent(typeof(PlayerAnimator))]
[RequireComponent(typeof(PlayerStatsRuntime))]
public class PlayerController : NetworkBehaviour
{
    [Header("Configuração de Stats")]
    [Tooltip("Arraste o ScriptableObject de PlayerStats para cá.")]
    public PlayerStats stats;

    // --- REFERÊNCIAS PÚBLICAS PARA OS ESPECIALISTAS ---
    public Rigidbody2D rb { get; private set; }
    public SpriteRenderer spriteRenderer { get; private set; }
    public Animator anim { get; private set; }
    public PlayerInput input { get; private set; }
    public PlayerMotor motor { get; private set; }
    public PlayerHealth health { get; private set; }
    public PlayerCombat combat { get; private set; }
    public PlayerAnimator playerAnimator { get; private set; }
    public PlayerStatsRuntime playerStatsRuntime { get; private set; }

    // Singleton para acesso FÁCIL e SEGURO apenas ao jogador LOCAL.
    public static PlayerController LocalInstance { get; private set; }

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
        input = GetComponent<PlayerInput>();
        motor = GetComponent<PlayerMotor>();
        health = GetComponent<PlayerHealth>();
        combat = GetComponent<PlayerCombat>();
        playerAnimator = GetComponent<PlayerAnimator>();
        playerStatsRuntime = GetComponent<PlayerStatsRuntime>();
    }

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            // Se este objeto me pertence, guardo-o como a minha instância local.
            // Scripts de UI e a câmara podem agora usar PlayerController.LocalInstance para encontrar o jogador.
            LocalInstance = this;
        }

        // A lógica de ativar/desativar o input já é gerida dentro do próprio PlayerInput.cs,
        // então não precisamos de a duplicar aqui.

        // A lógica de encontrar o spawn point foi removida. Esta responsabilidade
        // pertence ao script que cria (spawna) o jogador, como o ServerSceneManager.
    }

    // Limpa a instância quando o objeto é destruído para evitar referências nulas.
    public override void OnNetworkDespawn()
    {
        if (IsOwner)
        {
            if (LocalInstance == this)
            {
                LocalInstance = null;
            }
        }
    }
}