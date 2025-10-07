using UnityEngine;
using TMPro;
using Ink.Parsed;
using Unity.VisualScripting;

public class AttributePanelUI : MonoBehaviour
{
    [SerializeField] private PlayerStatsRuntime playerStats; // referência ao jogador

    [SerializeField] private int pointsForAttributes = 0;
    [SerializeField] private TextMeshProUGUI pointsForAttributesText;
    [SerializeField] private TextMeshProUGUI hpPointsText;
    [SerializeField] private TextMeshProUGUI mpPointsText;
    [SerializeField] private TextMeshProUGUI atkPointsText;
    [SerializeField] private TextMeshProUGUI defPointsText;
    [SerializeField] private TextMeshProUGUI spdPointsText;
    [SerializeField] private TextMeshProUGUI wisPointsText;
    [SerializeField] private TextMeshProUGUI dexPointsText;
    [SerializeField] private TextMeshProUGUI vitPointsText;


    void OnEnable()
    {
        GameEventsManager.Instance.playerEvents.OnLevelUp += UpdateAttributePanel;
        GameEventsManager.Instance.playerEvents.OnAttributeChanged += UpdateAttributePanel;
    }

    void OnDisable()
    {
        GameEventsManager.Instance.playerEvents.OnLevelUp -= UpdateAttributePanel;
        GameEventsManager.Instance.playerEvents.OnAttributeChanged -= UpdateAttributePanel;
    }

    void Awake()
    {
        UpdateAttributePanel();
    }

    public void UpdateAttributePanel(int pointsForAttributes = 0)
    {
        if (playerStats.available_attribute_points > 0)
        {
            pointsForAttributesText.text = playerStats.available_attribute_points.ToString() + " pontos disponíveis";
            pointsForAttributesText.gameObject.SetActive(true);
        }
        else
        {
            pointsForAttributesText.gameObject.SetActive(false);
        }

        Debug.Log("Painel de Atributos atualizado com " + pointsForAttributes + " pontos!");

        hpPointsText.text = "HP: " + playerStats.health.ToString();
        mpPointsText.text = "MP: " + playerStats.mana.ToString();
        atkPointsText.text = "ATK: " + playerStats.attack.ToString();
        defPointsText.text = "DEF: " + playerStats.defense.ToString();
        spdPointsText.text = "SPD: " + playerStats.speed.ToString();
        wisPointsText.text = "WIS: " + playerStats.wisdom.ToString();
        dexPointsText.text = "DEX: " + playerStats.dexterity.ToString();
        vitPointsText.text = "VIT: " + playerStats.vitality.ToString();
    }
    
    public void IncreaseAttribute(string attribute)
    {
        if (playerStats.available_attribute_points <= 0)
        {
            Debug.Log("Nenhum ponto de atributo disponível.");
            return;
        }
        Debug.Log("Aumentando atributo: " + attribute);
        GameEventsManager.Instance.playerEvents.AttributeIncreased(attribute);
    }
}
