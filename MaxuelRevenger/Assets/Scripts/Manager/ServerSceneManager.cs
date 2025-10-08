using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Netcode;
using System.Collections; // Necessário para Coroutines (IEnumerator)

public class ServerSceneManager : NetworkBehaviour
{
    public static ServerSceneManager Instance { get; private set; }

    // --- MUDANÇA 1: Adicionar Variáveis ---
    // Referência para o Prefab do jogador que vamos criar manualmente.
    [Header("Configuration")]
    [SerializeField] private GameObject playerPrefab;

    // Nome da cena para carregar. Tornar isto uma variável é mais flexível.
    [SerializeField] private string hubSceneName = "SampleScene"; // Verifique se o nome está EXATO.

    // Flag para sabermos se a cena principal já foi carregada.
    private bool isHubSceneLoaded = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    // --- MUDANÇA 2: Lógica de Spawn e Carregamento de Cena ---
    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;

        // Subscrevemos ao evento que é chamado sempre que um cliente se conecta.
        // Isto vai tratar tanto do Host (que se conecta a si mesmo) como dos clientes remotos.
        NetworkManager.Singleton.OnClientConnectedCallback += HandleClientConnected;

        // Em vez de carregar a cena diretamente, iniciamos uma Coroutine que
        // nos permite ESPERAR que o carregamento da cena termine.
        StartCoroutine(LoadHubScene());
    }

    private IEnumerator LoadHubScene()
    {
        // Usamos o SceneManager da Unity para ter mais controlo.
        // A operação de carregamento é assíncrona.
        var sceneLoadOperation = SceneManager.LoadSceneAsync(hubSceneName, LoadSceneMode.Additive);

        // Este loop é a chave: ele pausa a execução desta função
        // até que a cena esteja completamente carregada.
        while (!sceneLoadOperation.isDone)
        {
            yield return null;
        }

        // Agora que a cena terminou de carregar, podemos permitir que os jogadores sejam criados.
        isHubSceneLoaded = true;
    }

    // --- MUDANÇA 3: Método para Gerir Conexões de Clientes ---
    private void HandleClientConnected(ulong clientId)
    {
        // Nós só criamos o jogador se a cena HUB já estiver pronta.
        // Se um cliente se conectar muito rápido, esta função será chamada, mas o 'if' irá falhar.
        // A lógica de spawn para esse cliente será re-tentada ou tratada quando a cena carregar.
        // Uma abordagem mais simples é esperar que o 'isHubSceneLoaded' seja verdadeiro.
        StartCoroutine(WaitForSceneLoadAndSpawn(clientId));
    }

    private IEnumerator WaitForSceneLoadAndSpawn(ulong clientId)
    {
        // Espera até que a flag 'isHubSceneLoaded' se torne verdadeira.
        while (!isHubSceneLoaded)
        {
            yield return null;
        }

        // A cena está carregada, agora podemos criar o jogador com segurança.
        SpawnPlayer(clientId);
    }

    // --- MUDANÇA 4: Método Específico para o Spawn ---
    private void SpawnPlayer(ulong clientId)
    {
        // Se o prefab não foi atribuído, não faz nada para evitar erros.
        if (playerPrefab == null)
        {
            Debug.LogError("Player Prefab não está atribuído no ServerSceneManager!");
            return;
        }

        GameObject playerInstance = Instantiate(playerPrefab);
        // O SpawnAsPlayerObject associa este objeto ao cliente específico.
        playerInstance.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId, true);
    }

    // --- MUDANÇA 5: Limpeza de Eventos (Boa Prática) ---
    public override void OnNetworkDespawn()
    {
        if (IsServer && NetworkManager.Singleton != null)
        {
            // É importante remover a subscrição dos eventos quando o objeto é destruído
            // para evitar erros e memory leaks.
            NetworkManager.Singleton.OnClientConnectedCallback -= HandleClientConnected;
        }
    }
}