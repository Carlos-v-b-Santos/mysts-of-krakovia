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
    }

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;

        NetworkManager.Singleton.OnClientConnectedCallback += HandleClientConnected;
        StartCoroutine(LoadHubScene());
    }

    private IEnumerator LoadHubScene()
    {
        var sceneLoadOperation = SceneManager.LoadSceneAsync(hubSceneName, LoadSceneMode.Additive);

        while (!sceneLoadOperation.isDone)
        {
            yield return null;
        }

        isHubSceneLoaded = true;

        // ++ NOVA LÓGICA DE ATIVAÇÃO DO SPAWNER ++
        Debug.Log("Cena do Hub carregada. Procurando pelo EnemySpawner...");
        EnemySpawner spawner = FindFirstObjectByType<EnemySpawner>();
        if (spawner != null)
        {
            Debug.Log("EnemySpawner encontrado! Ativando o spawn.");
            spawner.StartSpawning();
        }
        else
        {
            Debug.LogError("CRÍTICO: Não foi possível encontrar o EnemySpawner na cena carregada! Os inimigos não serão criados.");
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
            Debug.LogError("Player Prefab não está atribuído no ServerSceneManager!");
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