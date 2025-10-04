using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RangeWeapon : MonoBehaviour
{
    [Header("Referências")]
    public GameObject projectilePrefab;
    public Transform muzzle; // Posição de spawn do projétil
    //public TMPro.TextMeshProUGUI ammoLabel; // UI de munição

    [Header("Configuração da Arma")]
    [SerializeField] private float cadenciaAtaque = 1f;
    [SerializeField] private int quantidadeMunicao = 12;
    [SerializeField] private int capacidadeMaximaMunicao = 12;
    [SerializeField] private int municaoCarregadaPorReload = 12;
    [SerializeField] private float tempoRecarga = 1f;
    [SerializeField] private int tier = 0;

    [Header("Atributos do Projétil")]
    [SerializeField] private int projDano = 1;
    [SerializeField] private float projVelocidade = 1f;
    [SerializeField] private float projDuracao = 3f;

    private bool podeDisparar = true;
    private bool isLoadingAmmo = false;
    private Transform target;

    // Módulos da arma (Composite Pattern)
    [SerializeField] private List<WeaponModuleSO> fireModules;

    void OnEnable()
    {
    }

    public void SetStats(int PlayerAttack, int PlayerDexterity)
    {
        projDano += PlayerAttack;
        cadenciaAtaque -= PlayerDexterity * 0.05f;
        if (cadenciaAtaque < 0.1f) cadenciaAtaque = 0.1f; // Limite mínimo de cadência
    }

    void Awake()
    {
        target = GetComponentInChildren<Aim>().transform;
        //UpdateAmmoLabel();
    }

    public void AddModule(WeaponModuleSO module)
    {
        //module.SetWeapon(this);
        fireModules.Add(module);
    }

    public void RemoveModule(WeaponModuleSO module)
    {
        fireModules.Remove(module);
        //module.SetWeapon(null);
    }

    public void Disparar(WeaponModuleSO except = null)
    {
        if (!podeDisparar || quantidadeMunicao <= 0 || isLoadingAmmo) return;

        //if (externalTarget.HasValue)
        //    target = externalTarget.Value;

        quantidadeMunicao--;

        var baseData = GetDefaultProjectileData();

        var modifiers = new Dictionary<string, float>()
        {
            {"dano", 1f},
            {"velocidade", 1f},
            {"duracao", 1f}
        };

        // Player modifiers (exemplo, precisa adaptar ao seu sistema de stats)
        // TODO: integrar com stats do jogador

        // Modificadores dos módulos
        foreach (var module in fireModules)
        {
            if (module != except)
                module.ModifyProjectile(modifiers);
        }

        // Aplica modificadores
        baseData["dano"] *= modifiers["dano"];
        baseData["velocidade"] *= modifiers["velocidade"];
        baseData["duracao"] *= modifiers["duracao"];

        // Se não tiver módulos, usa disparo padrão
        if (fireModules.Count == 0)
        {
            FireProjectile(baseData, GetFireDirection());
        }
        else
        {
            foreach (var module in fireModules)
            {
                if (module != except)
                    module.Execute(this, baseData);
            }
        }

        podeDisparar = false;

        if (quantidadeMunicao <= 0)
        {
            StartCoroutine(Reload());
        }
        else
        {
            StartCoroutine(FireCooldown());
        }

        //UpdateAmmoLabel();
    }

    private IEnumerator FireCooldown()
    {
        yield return new WaitForSeconds(cadenciaAtaque);
        podeDisparar = true;
    }

    private IEnumerator Reload()
    {
        Debug.Log("Recarregando...");
        isLoadingAmmo = true;
        yield return new WaitForSeconds(tempoRecarga);
        quantidadeMunicao = municaoCarregadaPorReload;
        isLoadingAmmo = false;
        podeDisparar = true;
        Debug.Log("Recarga completa!");
        //UpdateAmmoLabel();
    }

    public void FireProjectile(Dictionary<string, float> data, Vector2 direction)
    {
        if (projectilePrefab == null || muzzle == null) return;

        GameObject proj = Instantiate(projectilePrefab, muzzle.position, Quaternion.identity);
        Projectile projectile = proj.GetComponent<Projectile>();

        if (projectile != null)
        {
            projectile.SetTarget(direction);
            projectile.damage = Mathf.RoundToInt(data["dano"]);
            projectile.speed = data["velocidade"];
            projectile.duration = data["duracao"];
        }
    }

    public Vector2 GetFireDirection()
    {
        return (target.position - muzzle.position).normalized;
    }

    private Dictionary<string, float> GetDefaultProjectileData()
    {
        return new Dictionary<string, float>()
        {
            {"dano", projDano},
            {"velocidade", projVelocidade},
            {"duracao", projDuracao}
        };
    }

    //private void UpdateAmmoLabel()
    //{
    //    if (ammoLabel != null)
    //        ammoLabel.text = quantidadeMunicao.ToString();
    //}
}
