using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Netcode;
using System.Collections;

public class ServerSceneManager : NetworkBehaviour
{
    public static ServerSceneManager Instance { get; private set; }

    [Header("Configuration")]
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private string hubSceneName = "SampleScene";

    private bool isHubSceneLoaded = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        Debug.Log("[ServerSceneManager] Awake: Singleton instanciado.");
    }

    public override void OnNetworkSpawn()
    {
        Debug.Log("[ServerSceneManager] OnNetworkSpawn: Chamado.");
        if (!IsServer) return;

        Debug.Log("[ServerSceneManager] É o servidor. Iniciando processo de carregamento de cena.");
        NetworkManager.Singleton.OnClientConnectedCallback += HandleClientConnected;
        StartCoroutine(LoadHubScene());
    }

    private IEnumerator LoadHubScene()
    {
        Debug.Log($"[ServerSceneManager] Coroutine LoadHubScene: Iniciando carregamento da cena '{hubSceneName}'.");
        var sceneLoadOperation = SceneManager.LoadSceneAsync(hubSceneName, LoadSceneMode.Additive);

        while (!sceneLoadOperation.isDone)
        {
            yield return null;
        }

        isHubSceneLoaded = true;
        Debug.Log($"[ServerSceneManager] <color=green>CENA '{hubSceneName}' CARREGADA COM SUCESSO.</color>");

        Debug.Log("[ServerSceneManager] Procurando por EnemySpawner na cena...");
        EnemySpawner spawner = FindFirstObjectByType<EnemySpawner>();
        if (spawner != null)
        {
            Debug.Log("[ServerSceneManager] <color=cyan>EnemySpawner ENCONTRADO!</color> Chamando spawner.StartSpawning().");
            spawner.StartSpawning();
        }
        else
        {
            Debug.LogError("[ServerSceneManager] ERRO CRÍTICO: Não foi possível encontrar o EnemySpawner na cena carregada! Verifique se o objeto está ativo na hierarquia da cena '" + hubSceneName + "'.");
        }
    }

    private void HandleClientConnected(ulong clientId)
    {
        StartCoroutine(WaitForSceneLoadAndSpawn(clientId));
    }

    private IEnumerator WaitForSceneLoadAndSpawn(ulong clientId)
    {
        while (!isHubSceneLoaded)
        {
            yield return null;
        }
        SpawnPlayer(clientId);
    }

    private void SpawnPlayer(ulong clientId)
    {
        if (playerPrefab == null)
        {
            Debug.LogError("[ServerSceneManager] Player Prefab não está atribuído no ServerSceneManager!");
            return;
        }

        GameObject playerInstance = Instantiate(playerPrefab);
        playerInstance.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId, true);
    }

    public override void OnNetworkDespawn()
    {
        if (IsServer && NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= HandleClientConnected;
        }
    }
}