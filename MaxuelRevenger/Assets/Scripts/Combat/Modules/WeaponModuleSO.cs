using System.Collections.Generic;
using UnityEngine;

public abstract class WeaponModuleSO : ScriptableObject, IWeaponModule 
{
    public abstract void ModifyProjectile(Dictionary<string, float> data);
    public abstract void Execute(RangeWeapon weaponNode, Dictionary<string, float> data);
}