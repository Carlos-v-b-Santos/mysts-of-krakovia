using UnityEngine;

public class PlayerLevel : MonoBehaviour
{
    [SerializeField] private int level = 1;
    [SerializeField] private int current_xp = 0;
    [SerializeField] private int xp_needed = 100;
    [SerializeField] private int base_xp_needed = 100;
    [SerializeField] private float xp_increase_factor = 1.2f;
    [SerializeField] private int points_per_level = 10;
    [SerializeField] private int available_attribute_points = 0;

    private void OnEnable()
    {
        GameEventsManager.Instance.playerEvents.OnLevelUp += LevelUp;
        GameEventsManager.Instance.playerEvents.OnXPReceived += ReceberXP;
    }

    private void OnDisable()
    {
        GameEventsManager.Instance.playerEvents.OnLevelUp -= LevelUp;
        GameEventsManager.Instance.playerEvents.OnXPReceived -= ReceberXP;
    }

    public void LevelUp()
    {
        level++;
        IncreaseUpdateXpRequired();
        IncreasePointsForAttributes();
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
