using UnityEngine;
using TMPro;

public class AttributePanelUI : MonoBehaviour
{
    // A referência agora é para o PlayerStatsRuntime do jogador LOCAL.
    // Precisamos de uma forma de encontrar o jogador local.
    private PlayerStatsRuntime localPlayerStats;

    [SerializeField] private TextMeshProUGUI pointsForAttributesText;
    [SerializeField] private TextMeshProUGUI hpPointsText;
    [SerializeField] private TextMeshProUGUI mpPointsText;
    [SerializeField] private TextMeshProUGUI atkPointsText;
    [SerializeField] private TextMeshProUGUI defPointsText;
    [SerializeField] private TextMeshProUGUI spdPointsText;
    [SerializeField] private TextMeshProUGUI wisPointsText;
    [SerializeField] private TextMeshProUGUI dexPointsText;
    [SerializeField] private TextMeshProUGUI vitPointsText;

    // Tenta encontrar o jogador local e inscreve-se nos seus eventos.
    public void Initialize()
    {
        // Encontra a instância do jogador local que foi definida no PlayerController
        if (PlayerController.LocalInstance != null)
        {
            localPlayerStats = PlayerController.LocalInstance.GetComponent<PlayerStatsRuntime>();
            if (localPlayerStats != null)
            {
                // Inscreve-se no evento para ser notificado quando os stats mudarem.
                localPlayerStats.OnStatsChanged += UpdateAttributePanel;
                // Atualiza o painel uma vez no início.
                UpdateAttributePanel();
            }
        }
    }

    private void OnDisable()
    {
        // Limpa a inscrição para evitar erros.
        if (localPlayerStats != null)
        {
            localPlayerStats.OnStatsChanged -= UpdateAttributePanel;
        }
    }

    // Este método agora é chamado pelo evento OnStatsChanged.
    public void UpdateAttributePanel()
    {
        if (localPlayerStats == null) return;

        // --- CORREÇÃO: Usamos .Value para ler o valor das NetworkVariables ---
        if (localPlayerStats.available_attribute_points.Value > 0)
        {
            pointsForAttributesText.text = localPlayerStats.available_attribute_points.Value.ToString() + " pontos disponíveis";
            pointsForAttributesText.gameObject.SetActive(true);
        }
        else
        {
            pointsForAttributesText.gameObject.SetActive(false);
        }

        hpPointsText.text = "HP: " + localPlayerStats.health.Value.ToString();
        mpPointsText.text = "MP: " + localPlayerStats.mana.Value.ToString();
        atkPointsText.text = "ATK: " + localPlayerStats.attack.Value.ToString();
        defPointsText.text = "DEF: " + localPlayerStats.defense.Value.ToString();
        spdPointsText.text = "SPD: " + localPlayerStats.speed.Value.ToString();
        wisPointsText.text = "WIS: " + localPlayerStats.wisdom.Value.ToString();
        dexPointsText.text = "DEX: " + localPlayerStats.dexterity.Value.ToString();
        vitPointsText.text = "VIT: " + localPlayerStats.vitality.Value.ToString();
    }

    // Este método agora chama o ServerRpc no PlayerStatsRuntime.
    public void IncreaseAttribute(string attribute)
    {
        if (localPlayerStats == null || localPlayerStats.available_attribute_points.Value <= 0)
        {
            Debug.Log("Nenhum ponto de atributo disponível.");
            return;
        }

        Debug.Log("Pedindo ao servidor para aumentar o atributo: " + attribute);
        // Pede ao servidor para executar a lógica de aumento de atributo.
        localPlayerStats.IncreaseAttributeServerRpc(attribute);
    }
}