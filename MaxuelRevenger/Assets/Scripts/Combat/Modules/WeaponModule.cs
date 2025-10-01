using System.Collections.Generic;
using UnityEngine;

public interface IWeaponModule
{
    /// <summary>
    /// Permite ao módulo modificar atributos do projétil (ex: aumentar dano, mudar velocidade).
    /// </summary>
    public abstract void ModifyProjectile(Dictionary<string, float> data);

    /// <summary>
    /// Executa a lógica do módulo ao disparar (ex: disparar múltiplos projéteis).
    /// </summary>
    public abstract void Execute(RangeWeapon weaponNode, Dictionary<string, float> data);
}
