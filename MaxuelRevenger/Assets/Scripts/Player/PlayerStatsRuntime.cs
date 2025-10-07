using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStatsRuntime : MonoBehaviour
{
    [SerializeField] public int max_health { get; private set; } = 100;
    [SerializeField] public int current_health { get; private set; } = 100;
    [SerializeField] public int max_mana { get; private set; } = 50;
    [SerializeField] public int current_mana { get; private set; } = 50;
    [SerializeField] public int health { get; private set; } = 1;
    [SerializeField] public int mana { get; private set; } = 1;
    [SerializeField] public int attack { get; private set; } = 1;
    [SerializeField] public int defense { get; private set; } = 1;
    [SerializeField] public int speed { get; private set; } = 1; // velocidade de movimento
    [SerializeField] public int wisdom { get; private set; } = 1; // regen de mana
    [SerializeField] public int dexterity { get; private set; } = 1; // velocidade de ataque
    [SerializeField] public int vitality { get; private set; } = 1; // regen de vida
    [SerializeField] public int available_attribute_points { get; private set; } = 0;
    [SerializeField] private int level = 1;
    [SerializeField] private int current_xp = 0;
    [SerializeField] private int xp_needed = 100;
    [SerializeField] private int base_xp_needed = 100;
    [SerializeField] private float xp_increase_factor = 1.2f;
    [SerializeField] private int points_per_level = 10;
    

    PlayerStats playerStats;

    public void OnEnable()
    {
        GameEventsManager.Instance.playerEvents.OnAttributeIncreased += IncreaseAttribute;
        GameEventsManager.Instance.playerEvents.OnXPReceived += ReceberXP;
    }

    public void OnDisable()
    {
        GameEventsManager.Instance.playerEvents.OnAttributeIncreased -= IncreaseAttribute;
        GameEventsManager.Instance.playerEvents.OnXPReceived -= ReceberXP;
    }

    // Start is called before the first frame update
    void Start()
    {
        playerStats = ScriptableObject.CreateInstance<PlayerStats>();
        playerStats.InitializeBaseStats();
    }

    private void IncreaseAttribute(string attribute)
    {
        switch (attribute)
        {
            case "hp":
                health++;
                max_health += 10; // Aumenta a vida m��xima em 10 a cada ponto em "health"
                current_health = max_health; // Restaura a vida atual para o m��ximo
                break;
            case "mp":
                mana++;
                max_mana += 5; // Aumenta a mana m��xima em 5 a cada ponto em "mana"
                current_mana = max_mana; // Restaura a mana atual para o m��ximo
                break;
            case "atk":
                attack++;
                break;
            case "def":
                defense++;
                break;
            case "spd":
                speed++;
                break;
            case "wis":
                wisdom++;
                break;
            case "dex":
                dexterity++;
                break;
            case "vit":
                vitality++;
                break;
            default:
                Debug.LogWarning("Atributo desconhecido: " + attribute);
                break;
        }
        available_attribute_points--;
        GameEventsManager.Instance.playerEvents.AttributeChanged(0);
    }

    
    public void LevelUp()
    {
        level++;
        IncreaseUpdateXpRequired();
        IncreasePointsForAttributes();
        GameEventsManager.Instance.playerEvents.LevelUp(points_per_level);
        Debug.Log($"Subiu para o n��vel {level}! XP necess��rio para o pr��ximo n��vel: {xp_needed}");
        // Aqui voc�� pode adicionar l��gica adicional, como aumentar atributos do jogador, conceder pontos de habilidade, etc.
    }

    public void IncreasePointsForAttributes()
    {
        // Aqui você pode adicionar lógica para aumentar os pontos de atributos do jogador  
        available_attribute_points += points_per_level;
    }

    public void ReceberXP(int amount)
    {
        current_xp += amount;
        Debug.Log($"XP recebido: {amount}. XP atual: {current_xp}/{xp_needed}");

        // Verifica se o jogador tem XP suficiente para subir de n��vel
        if (current_xp >= xp_needed)
        {
            current_xp -= xp_needed; // Mantem o XP que excedeu o necess��rio
            LevelUp();
        }
    }

    public void IncreaseUpdateXpRequired()
    {
        xp_needed = Mathf.RoundToInt(base_xp_needed * Mathf.Pow(level, xp_increase_factor));
    }
}
