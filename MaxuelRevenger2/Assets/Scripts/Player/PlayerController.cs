using UnityEngine;

// Garante que todos os componentes necessários estejam no GameObject
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(PlayerMotor))]
[RequireComponent(typeof(PlayerHealth))]
[RequireComponent(typeof(PlayerCombat))]
[RequireComponent(typeof(PlayerAnimator))]
public class PlayerController : MonoBehaviour
{
    [Header("Stats Configuration")]
    [Tooltip("Arraste o ScriptableObject de PlayerStats para cá.")]
    public PlayerStats stats; // <-- Referência para o seu asset de status!

    // Referências públicas para os outros componentes
    public Rigidbody2D rb { get; private set; }
    public SpriteRenderer spriteRenderer { get; private set; }
    public Animator anim { get; private set; }
    public PlayerMotor motor { get; private set; }
    public PlayerHealth health { get; private set; }
    public PlayerCombat combat { get; private set; }
    public PlayerAnimator playerAnimator { get; private set; }

    // Singleton para acesso global fácil
    public static PlayerController Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // Pegar referências dos componentes no GameObject
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
        motor = GetComponent<PlayerMotor>();
        health = GetComponent<PlayerHealth>();
        combat = GetComponent<PlayerCombat>();
        playerAnimator = GetComponent<PlayerAnimator>();
    }
}