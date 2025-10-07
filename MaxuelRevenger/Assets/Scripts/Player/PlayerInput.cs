using System;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode; // Importe o Netcode para herdar de NetworkBehaviour

// A classe agora herda de NetworkBehaviour e implementa a interface de callbacks do Input System.
public class PlayerInput : NetworkBehaviour, PlayerControls.IGameplayActions
{
    // --- EVENTOS P�BLICOS ---
    // Outros componentes no mesmo GameObject (Motor, Combat, Animator) ir�o "ouvir" estes an�ncios.
    public event Action OnJumpPressed;
    public event Action OnAttackPressed;
    public event Action<float> OnMoveEvent;
    public event Action OnInteractPressed;

    // Refer�ncia para a classe de controlos gerada pelo Input System.
    private PlayerControls playerControls;

    // Awake � chamado sempre.
    private void Awake()
    {
        // Cria a inst�ncia dos controlos.
        playerControls = new PlayerControls();
    }

    // OnNetworkSpawn � o "Start" para objetos de rede. � aqui que a l�gica de rede come�a.
    public override void OnNetworkSpawn()
    {
        // A "cl�usula de guarda" mais importante do multiplayer:
        // S� ative os controlos se este objeto de jogador me pertencer (IsOwner).
        if (!IsOwner) return;

        // Se sou o dono, ativo o mapa de a��es "Gameplay" e digo-lhe que este script
        // (this) � o respons�vel por receber os callbacks (as chamadas de fun��o).
        playerControls.Gameplay.Enable();
        playerControls.Gameplay.SetCallbacks(this);
    }

    // OnNetworkDespawn � chamado quando o objeto � removido da rede.
    public override void OnNetworkDespawn()
    {
        // Apenas desative os controlos se eu for o dono.
        if (IsOwner)
        {
            playerControls.Gameplay.Disable();
        }
    }


    // --- M�TODOS DE CALLBACK (CHAMADOS AUTOMATICAMENTE PELO INPUT SYSTEM) ---

    public void OnMove(InputAction.CallbackContext context)
    {
        // L� o valor do eixo de movimento (-1 para a esquerda, 1 para a direita).
        float moveDirection = context.ReadValue<float>();
        // Anuncia o evento OnMove com a dire��o, para quem estiver a ouvir (o PlayerMotor).
        OnMoveEvent?.Invoke(moveDirection);
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        // O evento "performed" acontece no momento em que o bot�o � pressionado.
        if (context.performed)
        {
            // Anuncia que o bot�o de pulo foi pressionado.
            OnJumpPressed?.Invoke();
        }
    }

    public void OnAttack(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            // Anuncia que o bot�o de ataque foi pressionado.
            OnAttackPressed?.Invoke();
        }
    }

    // Adicione este novo m�todo de callback
    public void OnInteract(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            OnInteractPressed?.Invoke(); // Anuncia que o bot�o de intera��o foi pressionado
        }
    }

    public void OnQuestLogToggle(InputAction.CallbackContext context) { }
    public void OnShoot(InputAction.CallbackContext context) { }
}