using UnityEngine;

public class DamageManager : MonoBehaviour
{
    public static DamageManager Instance { get; private set; }
    
    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("Found more than one Damage Manager in the scene.");
        }
        Instance = this;
    }

    public void ApplyDamage(GameObject target, int amount, int ownerId)
    {
        // Procura um componente que implemente IDamageable
        var damageable = target.GetComponent<IDamageable>();

        if (damageable != null)
        {
            damageable.TakeDamage(amount, ownerId);
            Debug.Log($"Dano aplicado: {amount} em {target.name}");
        }
        else
        {
            Debug.LogWarning($"Objeto {target.name} não implementa IDamageable");
        }
    }
}