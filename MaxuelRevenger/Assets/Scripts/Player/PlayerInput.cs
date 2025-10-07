using System;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode; // Importe o Netcode para herdar de NetworkBehaviour

// A classe agora herda de NetworkBehaviour e implementa a interface de callbacks do Input System.
public class PlayerInput : NetworkBehaviour, PlayerControls.IGameplayActions
{
    // --- EVENTOS PÚBLICOS ---
    // Outros componentes no mesmo GameObject (Motor, Combat, Animator) irão "ouvir" estes anúncios.
    public event Action OnJumpPressed;
    public event Action OnAttackPressed;
    public event Action<float> OnMoveEvent;
    public event Action OnInteractPressed;

    // Referência para a classe de controlos gerada pelo Input System.
    private PlayerControls playerControls;

    // Awake é chamado sempre.
    private void Awake()
    {
        // Cria a instância dos controlos.
        playerControls = new PlayerControls();
    }

    // OnNetworkSpawn é o "Start" para objetos de rede. É aqui que a lógica de rede começa.
    public override void OnNetworkSpawn()
    {
        // A "cláusula de guarda" mais importante do multiplayer:
        // Só ative os controlos se este objeto de jogador me pertencer (IsOwner).
        if (!IsOwner) return;

        // Se sou o dono, ativo o mapa de ações "Gameplay" e digo-lhe que este script
        // (this) é o responsável por receber os callbacks (as chamadas de função).
        playerControls.Gameplay.Enable();
        playerControls.Gameplay.SetCallbacks(this);
    }

    // OnNetworkDespawn é chamado quando o objeto é removido da rede.
    public override void OnNetworkDespawn()
    {
        // Apenas desative os controlos se eu for o dono.
        if (IsOwner)
        {
            playerControls.Gameplay.Disable();
        }
    }


    // --- MÉTODOS DE CALLBACK (CHAMADOS AUTOMATICAMENTE PELO INPUT SYSTEM) ---

    public void OnMove(InputAction.CallbackContext context)
    {
        // Lê o valor do eixo de movimento (-1 para a esquerda, 1 para a direita).
        float moveDirection = context.ReadValue<float>();
        // Anuncia o evento OnMove com a direção, para quem estiver a ouvir (o PlayerMotor).
        OnMoveEvent?.Invoke(moveDirection);
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        // O evento "performed" acontece no momento em que o botão é pressionado.
        if (context.performed)
        {
            // Anuncia que o botão de pulo foi pressionado.
            OnJumpPressed?.Invoke();
        }
    }

    public void OnAttack(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            // Anuncia que o botão de ataque foi pressionado.
            OnAttackPressed?.Invoke();
        }
    }

    // Adicione este novo método de callback
    public void OnInteract(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            OnInteractPressed?.Invoke(); // Anuncia que o botão de interação foi pressionado
        }
    }

    public void OnQuestLogToggle(InputAction.CallbackContext context) { }
    public void OnShoot(InputAction.CallbackContext context) { }
}