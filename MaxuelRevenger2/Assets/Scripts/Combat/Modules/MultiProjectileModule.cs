using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MultiProjectileModule", menuName = "ScriptableObjects/MultiProjectileModule", order = 1)]
public class MultiProjectileModule : WeaponModuleSO
{
    public int numProjectiles = 5;
    public float delayBetweenShots = 0.15f;

    public override void ModifyProjectile(Dictionary<string, float> modifiers)
    {
        return; // Não modifica atributos do projétil
    }

    public override void Execute(RangeWeapon weapon, Dictionary<string, float> data)
    {
        // Inicia uma coroutine para disparar vários projéteis
        weapon.StartCoroutine(FireMultipleProjectiles(weapon, data));
    }

    private IEnumerator FireMultipleProjectiles(RangeWeapon weapon, Dictionary<string, float> data)
    {
        Vector2 baseDirection = weapon.GetFireDirection();

        for (int i = 0; i < numProjectiles; i++)
        {
            // Chamada equivalente ao "disparar(self)" no Godot
            weapon.Disparar(this);

            // Dispara o projétil
            weapon.FireProjectile(data, baseDirection);

            // Delay entre disparos
            yield return new WaitForSeconds(delayBetweenShots);
        }
    }
}