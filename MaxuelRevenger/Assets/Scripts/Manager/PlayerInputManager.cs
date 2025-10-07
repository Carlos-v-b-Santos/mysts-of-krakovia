using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputManager : MonoBehaviour, PlayerControls.IUIActions
{
    private PlayerControls playerControls;

    // Evento para anunciar que o botão "Submit" (confirmar) foi pressionado.
    public event Action OnSubmitPressed;

    // Singleton para acesso global
    private static PlayerInputManager instance;
    public static PlayerInputManager GetInstance()
    {
        return instance;
    }

    private void Awake()
    {
        if (instance != null)
        {
            Debug.LogError("Found more than one Input Manager in the scene.");
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);

        playerControls = new PlayerControls();
    }

    private void OnEnable()
    {
        // O gestor global sempre "ouve" os controlos da UI.
        playerControls.UI.Enable();
        playerControls.UI.SetCallbacks(this);
    }

    private void OnDisable()
    {
        playerControls.UI.Disable();
        playerControls.UI.RemoveCallbacks(this);
    }

    // --- MÉTODOS PÚBLICOS PARA TROCAR DE MODO ---

    // Chamado quando um diálogo começa
    public void EnterMenuMode()
    {
        playerControls.Gameplay.Disable(); // Desativa os controlos de gameplay
        playerControls.UI.Enable();        // Ativa os controlos de UI
    }

    // Chamado quando um diálogo termina
    public void EnterGameplayMode()
    {
        playerControls.UI.Disable();          // Desativa os controlos de UI
        playerControls.Gameplay.Enable();     // Ativa os controlos de gameplay
    }

    // --- CALLBACKS DA INTERFACE DE UI ---

    public void OnSubmit(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            // Anuncia o evento para quem estiver a "ouvir" (ex: o seu sistema de diálogo)
            OnSubmitPressed?.Invoke();
        }
    }

    public void OnNavigate(InputAction.CallbackContext context)
    {
        // Pode ser usado para navegar em menus com setas/analógico
    }

    public void OnQuestLogToggle(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            GameEventsManager.Instance.inputEvents.QuestLogTogglePressed();
        }
    }
}