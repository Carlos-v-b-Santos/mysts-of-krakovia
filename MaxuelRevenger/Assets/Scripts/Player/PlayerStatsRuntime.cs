using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStatsRuntime : MonoBehaviour
{
    [SerializeField] public int max_health { get; private set; } = 100;
    [SerializeField] public int current_health { get; private set; } = 100;
    [SerializeField] public int max_mana { get; private set; } = 50;
    [SerializeField] public int current_mana { get; private set; } = 50;
    [SerializeField] public int attack { get; private set; } = 1;
    [SerializeField] public int defense { get; private set; } = 1;
    [SerializeField] public int speed { get; private set; } = 1; // velocidade de movimento
    [SerializeField] public int wisdom { get; private set; } = 1; // regen de mana
    [SerializeField] public int dexterity { get; private set; } = 1; // velocidade de ataque
    [SerializeField] public int vitality { get; private set; } = 1; // regen de vida

    PlayerStats playerStats;

    // Start is called before the first frame update
    void Start()
    {
        playerStats = ScriptableObject.CreateInstance<PlayerStats>();
        playerStats.InitializeBaseStats();
    }
}
