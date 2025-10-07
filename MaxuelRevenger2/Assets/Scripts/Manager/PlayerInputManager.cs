using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

public class PlayerInputManager : MonoBehaviour, PlayerControls.IGameplayActions, PlayerControls.IUIActions
{
    private PlayerControls playerControls;

    // Propriedades públicas para expor interacao
    public Vector2 MoveInput { get; private set; }
    public bool JumpPressed { get; private set; }
    public bool AttackPressed { get; private set; }
    public bool SubmitPressed { get; private set; } //confirmar opções nos dialogos
    public bool InteractPressed { get; private set; } //interagir com npcs e objetos

    private bool isFiring;


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
        }
        instance = this;

        playerControls = new PlayerControls();
    }

    public void OnAttack(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            Debug.Log("Attack performed");
            GameEventsManager.Instance.inputEvents.AttackPressed();
        }
    }

    public void OnInteract(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            Debug.Log("Interact performed");
            GameEventsManager.Instance.inputEvents.InteractPressed();
        }
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            Debug.Log("Jump performed");
            GameEventsManager.Instance.inputEvents.JumpPressed();
        }
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        if (context.performed || context.canceled)
        {
            float moveDir = context.ReadValue<float>();
            Debug.Log("Move performed: " + moveDir);
            GameEventsManager.Instance.inputEvents.MovePressed(moveDir);
        }
    }

    public void OnQuestLogToggle(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            Debug.Log("QuestLogToggle performed");
            GameEventsManager.Instance.inputEvents.QuestLogTogglePressed();
        }
    }

    public void OnShoot(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            Debug.Log("Shoot performed");
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            {
                // Ignore shoot input if the pointer is over a UI element
                Debug.Log("Pointer is over UI, ignoring shoot input");
                return;
            }

            GameEventsManager.Instance.inputEvents.ShootPressed();
        }
        else if (context.canceled)
        {
            Debug.Log("Shoot canceled");
            GameEventsManager.Instance.inputEvents.ShootReleased();
        }
    }

    public void OnSubmit(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            Debug.Log("Submit performed");
            GameEventsManager.Instance.inputEvents.SubmitPressed();
            SubmitPressed = true;
        }
    }

    public bool GetSubmitPressed()
    {
        bool result = SubmitPressed;
        SubmitPressed = false;
        return result;
    }

    public void EnterMenuMode()
    {
        DisablePlayerControls();
        EnableUIControls();
    }

    public void EnterGameplayMode()
    {
        EnablePlayerControls();
        DisableUIControls();
    }

    private void EnablePlayerControls()
    {
        playerControls.Gameplay.Enable();
        playerControls.Gameplay.SetCallbacks(this);
    }

    private void DisablePlayerControls()
    {
        playerControls.Gameplay.Disable();
        playerControls.Gameplay.RemoveCallbacks(this);
    }

    private void EnableUIControls()
    {
        print("Enable UI Controls");
        playerControls.UI.Enable();
        //playerControls.UI.Submit.performed += ctx => SubmitPressed = true;
        //print(SubmitPressed);
        playerControls.UI.SetCallbacks(this);
    }

    private void DisableUIControls()
    {
        playerControls.UI.Disable();
        //playerControls.UI.Submit.performed -= ctx => SubmitPressedAgain();
        playerControls.UI.RemoveCallbacks(this);
    }

    public void RegisterSubmitPressed()
    {
        SubmitPressed = false;
    }

    public void OnNavigate(InputAction.CallbackContext context)
    {
        //throw new System.NotImplementedException();
    }

    private void OnEnable()
    {
        playerControls.Enable();
        EnterGameplayMode();

        // associar eventos de input
        playerControls.Gameplay.Jump.performed += OnJump;
        playerControls.Gameplay.Attack.performed += OnAttack;
        playerControls.Gameplay.Interact.performed += OnInteract;
        playerControls.Gameplay.QuestLogToggle.performed += OnQuestLogToggle;
        playerControls.UI.Submit.performed += OnSubmit;
    }

    private void OnDisable()
    {
        playerControls.Disable();

        // desinscrever de eventos input
        playerControls.Gameplay.Jump.performed -= OnJump;
        playerControls.Gameplay.Attack.performed -= OnAttack;
        playerControls.Gameplay.Interact.performed -= OnInteract;
        playerControls.Gameplay.QuestLogToggle.performed -= OnQuestLogToggle;
        playerControls.UI.Submit.performed -= OnSubmit;
    }

    // Associar método à input
    public void UseJumpInput() => JumpPressed = false;
    public void UseAttackInput() => AttackPressed = false;
    public void UseSubmitInput() => SubmitPressed = false;

}
