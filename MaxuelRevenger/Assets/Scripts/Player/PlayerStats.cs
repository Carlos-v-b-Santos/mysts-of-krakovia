using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerStats", menuName = "ScriptableObjects/PlayerStats", order = 1)]
public class PlayerStats : ScriptableObject
{
    [SerializeField] private int max_health = 100;
    [SerializeField] private int current_health = 100;
    [SerializeField] private int max_mana = 50;
    [SerializeField] private int current_mana = 50;
    [SerializeField] private int attack = 1;
    [SerializeField] private int defense = 1;
    [SerializeField] private int speed = 1; // velocidade de movimento
    [SerializeField] private int wisdom = 1; // regen de mana
    [SerializeField] private int dexterity = 1; // velocidade de ataque
    [SerializeField] private int vitality = 1; // regen de vida

    // Este método é útil para resetar/inicializar os atributos
    public void InitializeBaseStats()
    {
        current_health = max_health;
        current_mana = max_mana;
        attack = 1;
        defense = 1;
        speed = 1;
        wisdom = 1;
        dexterity = 1;
        vitality = 1;
    }
}