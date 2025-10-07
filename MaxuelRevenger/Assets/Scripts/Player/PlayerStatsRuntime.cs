using UnityEngine;
using Unity.Netcode;
using System; // Adicionado para usar Actions (eventos)

public class PlayerStatsRuntime : NetworkBehaviour
{
    // --- EVENTOS PARA A UI E OUTROS SISTEMAS ---
    // A UI irá ouvir estes eventos para se atualizar, em vez de verificar a cada frame.
    public event Action OnStatsChanged;

    // --- ATRIBUTOS SINCRONIZADOS ---
    // Todos os atributos que podem mudar durante o jogo e precisam de ser vistos por todos
    // são transformados em NetworkVariables. A permissão de escrita é sempre do Servidor.

    [Header("Recursos Sincronizados")]
    public NetworkVariable<int> current_health = new NetworkVariable<int>(100, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<int> max_health = new NetworkVariable<int>(100, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<int> current_mana = new NetworkVariable<int>(50, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<int> max_mana = new NetworkVariable<int>(50, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    [Header("Atributos Sincronizados")]
    public NetworkVariable<int> health = new NetworkVariable<int>(1, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<int> mana = new NetworkVariable<int>(1, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<int> attack = new NetworkVariable<int>(1, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<int> defense = new NetworkVariable<int>(1, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<int> speed = new NetworkVariable<int>(8, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server); // Defina o valor base aqui
    public NetworkVariable<int> wisdom = new NetworkVariable<int>(1, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<int> dexterity = new NetworkVariable<int>(1, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<int> vitality = new NetworkVariable<int>(1, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    [Header("Progressão Sincronizada")]
    public NetworkVariable<int> level = new NetworkVariable<int>(1, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<int> current_xp = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<int> xp_needed = new NetworkVariable<int>(100, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<int> available_attribute_points = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    // --- LÓGICA DE REDE ---

    public override void OnNetworkSpawn()
    {
        // Quando o jogador entra na rede, inicializa os seus atributos no servidor.
        if (IsServer)
        {
            // Note que agora usamos ".Value" para aceder ou modificar o valor de uma NetworkVariable
            current_health.Value = max_health.Value;
            current_mana.Value = max_mana.Value;
        }
    }

    // O cliente chama este método, mas ele corre APENAS NO SERVIDOR.
    [ServerRpc]
    public void IncreaseAttributeServerRpc(string attribute)
    {
        // O servidor valida se o jogador pode realmente aumentar o atributo
        if (available_attribute_points.Value <= 0) return;

        available_attribute_points.Value--;

        switch (attribute)
        {
            case "hp":
                health.Value++;
                max_health.Value += 10;
                current_health.Value = max_health.Value; // Recupera a vida ao aumentar
                break;
            case "mp":
                mana.Value++;
                max_mana.Value += 5;
                current_mana.Value = max_mana.Value;
                break;
            case "atk":
                attack.Value++;
                break;
            case "def":
                defense.Value++;
                break;
            case "spd":
                speed.Value++;
                break;
            case "wis":
                wisdom.Value++;
                break;
            case "dex":
                dexterity.Value++;
                break;
            case "vit":
                vitality.Value++;
                break;
            default:
                Debug.LogWarning("Atributo desconhecido: " + attribute);
                break;
        }

        // Anuncia que os atributos mudaram para que a UI possa atualizar-se
        OnStatsChanged?.Invoke();
    }

    // A sua lógica de XP e Level Up já estava correta, apenas precisamos de usar ".Value"
    public void ReceberXP(int amount)
    {
        if (!IsServer) return; // Proteção para garantir que só o servidor dá XP

        current_xp.Value += amount;

        if (current_xp.Value >= xp_needed.Value)
        {
            LevelUp();
        }
    }

    private void LevelUp()
    {
        // Este método só é chamado pelo servidor, por isso não precisa de proteção interna.
        current_xp.Value -= xp_needed.Value;
        level.Value++;
        available_attribute_points.Value += 10; // Exemplo de pontos por nível

        // Lógica para calcular o novo XP necessário...
        xp_needed.Value = Mathf.RoundToInt(100 * Mathf.Pow(level.Value, 1.2f));

        // Anuncia o Level Up para que a UI e outros sistemas possam reagir
        OnStatsChanged?.Invoke();
        Debug.Log($"Jogador {OwnerClientId} subiu para o nível {level.Value}!");
    }
}