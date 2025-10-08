using UnityEngine;
using Unity.Netcode; // Importe o Netcode

// O atributo [RequireComponent] é uma salvaguarda que força a Unity a adicionar
// estes componentes automaticamente se eles estiverem em falta.
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(PlayerInput))]
[RequireComponent(typeof(PlayerMotor))]
[RequireComponent(typeof(PlayerHealth))]
[RequireComponent(typeof(PlayerCombat))]
[RequireComponent(typeof(PlayerAnimator))]
[RequireComponent(typeof(PlayerStatsRuntime))]
public class PlayerController : NetworkBehaviour // Herda de NetworkBehaviour
{
    [Header("Configuração de Stats")]
    [Tooltip("Arraste o ScriptableObject de PlayerStats para cá.")]
    public PlayerStats stats; // A referência para os seus stats base

    // --- REFERÊNCIAS PÚBLICAS PARA OS ESPECIALISTAS ---
    // Outros scripts podem aceder a estes componentes de forma segura através do PlayerController.
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
        // "Apanha" as referências para todos os componentes especialistas no mesmo GameObject.
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

    // OnNetworkSpawn é o "Start" para objetos de rede.
    public override void OnNetworkSpawn()
    {
        // A lógica mais importante do multiplayer!
        if (IsOwner)
        {
            // Se este objeto me pertence, guardo-o como a minha instância local.
            // Scripts de UI e a câmara podem agora usar PlayerController.LocalInstance para encontrar o jogador.
            LocalInstance = this;

            // Ativa os componentes que só o jogador local deve correr (o input).
            input.enabled = true;

            // Faz a câmara seguir apenas o jogador local.
            CameraFollow cameraFollow = Camera.main.GetComponent<CameraFollow>();
            if (cameraFollow != null)
            {
                cameraFollow.target = this.transform;
            }
        }
        else
        {
            // Se este objeto não me pertence, eu desativo os seus componentes de controlo.
            // O seu movimento será sincronizado pelo ClientNetworkTransform.
            input.enabled = false;
        }
        // Apenas o servidor deve definir a posição inicial do jogador
        if (IsServer)
        {
            GameObject spawnPoint = GameObject.Find("SpawnPoint");
            if (spawnPoint != null)
            {
                transform.position = spawnPoint.transform.position;
            }
        }
    }
}