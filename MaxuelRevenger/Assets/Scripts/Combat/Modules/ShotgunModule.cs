using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ShotgunModule", menuName = "ScriptableObjects/ShotgunModule", order = 1)]
public class ShotgunModule : WeaponModuleSO
{
    public int numPellets = 5;
    public float spreadAngleDeg = 30f;

    public float duracaoModify = 0.1f;   // porcentagem
    public float velocidadeModify = 0.2f; // porcentagem

    public override void ModifyProjectile(Dictionary<string, float> modifiers)
    {
        modifiers["duracao"] *= duracaoModify;   // reduz duração
        modifiers["velocidade"] *= velocidadeModify; // projéteis mais lentos
    }

    public override void Execute(RangeWeapon weapon, Dictionary<string, float> data)
    {
        Vector2 baseDirection = weapon.GetFireDirection();

        float startAngle = -spreadAngleDeg / 2f;
        float angleStep = spreadAngleDeg / (numPellets - 1);

        for (int i = 0; i < numPellets; i++)
        {
            Debug.Log("Disparando pellet " + (i + 1));
            float angleOffset = startAngle + i * angleStep;
            Vector2 direction = RotateVector(baseDirection, angleOffset);

            weapon.FireProjectile(data, direction);
        }
    }

    // Helper para rotacionar um vetor em graus
    private Vector2 RotateVector(Vector2 v, float angleDeg)
    {
        float rad = angleDeg * Mathf.Deg2Rad;
        float cos = Mathf.Cos(rad);
        float sin = Mathf.Sin(rad);

        return new Vector2(
            v.x * cos - v.y * sin,
            v.x * sin + v.y * cos
        );
    }
}