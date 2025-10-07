using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "LongShotModule", menuName = "ScriptableObjects/LongShotModule", order = 1)]
public class LongShotModule : WeaponModuleSO
{
    [Header("Modificadores (% multiplicativo)")]
    public float duracaoModify = 0.3f;
    public float velocidadeModify = 3.0f;

    public override void ModifyProjectile(Dictionary<string, float> modifiers)
    {
        modifiers["duracao"] *= duracaoModify;
        modifiers["velocidade"] *= velocidadeModify;
    }

    public override void Execute(RangeWeapon weaponNode, Dictionary<string, float> data)
    {
        Vector2 baseDirection = weaponNode.GetFireDirection();
        weaponNode.FireProjectile(data, baseDirection);
    }
}