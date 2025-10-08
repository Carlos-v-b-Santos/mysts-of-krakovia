using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Netcode;
using System.Collections; // Necess�rio para Coroutines (IEnumerator)

public class ServerSceneManager : NetworkBehaviour
{
    public static ServerSceneManager Instance { get; private set; }

    // --- MUDAN�A 1: Adicionar Vari�veis ---
    // Refer�ncia para o Prefab do jogador que vamos criar manualmente.
    [Header("Configuration")]
    [SerializeField] private GameObject playerPrefab;

    // Nome da cena para carregar. Tornar isto uma vari�vel � mais flex�vel.
    [SerializeField] private string hubSceneName = "SampleScene"; // Verifique se o nome est� EXATO.

    // Flag para sabermos se a cena principal j� foi carregada.
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

    // --- MUDAN�A 2: L�gica de Spawn e Carregamento de Cena ---
    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;

        // Subscrevemos ao evento que � chamado sempre que um cliente se conecta.
        // Isto vai tratar tanto do Host (que se conecta a si mesmo) como dos clientes remotos.
        NetworkManager.Singleton.OnClientConnectedCallback += HandleClientConnected;

        // Em vez de carregar a cena diretamente, iniciamos uma Coroutine que
        // nos permite ESPERAR que o carregamento da cena termine.
        StartCoroutine(LoadHubScene());
    }

    private IEnumerator LoadHubScene()
    {
        // Usamos o SceneManager da Unity para ter mais controlo.
        // A opera��o de carregamento � ass�ncrona.
        var sceneLoadOperation = SceneManager.LoadSceneAsync(hubSceneName, LoadSceneMode.Additive);

        // Este loop � a chave: ele pausa a execu��o desta fun��o
        // at� que a cena esteja completamente carregada.
        while (!sceneLoadOperation.isDone)
        {
            yield return null;
        }

        // Agora que a cena terminou de carregar, podemos permitir que os jogadores sejam criados.
        isHubSceneLoaded = true;
    }

    // --- MUDAN�A 3: M�todo para Gerir Conex�es de Clientes ---
    private void HandleClientConnected(ulong clientId)
    {
        // N�s s� criamos o jogador se a cena HUB j� estiver pronta.
        // Se um cliente se conectar muito r�pido, esta fun��o ser� chamada, mas o 'if' ir� falhar.
        // A l�gica de spawn para esse cliente ser� re-tentada ou tratada quando a cena carregar.
        // Uma abordagem mais simples � esperar que o 'isHubSceneLoaded' seja verdadeiro.
        StartCoroutine(WaitForSceneLoadAndSpawn(clientId));
    }

    private IEnumerator WaitForSceneLoadAndSpawn(ulong clientId)
    {
        // Espera at� que a flag 'isHubSceneLoaded' se torne verdadeira.
        while (!isHubSceneLoaded)
        {
            yield return null;
        }

        // A cena est� carregada, agora podemos criar o jogador com seguran�a.
        SpawnPlayer(clientId);
    }

    // --- MUDAN�A 4: M�todo Espec�fico para o Spawn ---
    private void SpawnPlayer(ulong clientId)
    {
        // Se o prefab n�o foi atribu�do, n�o faz nada para evitar erros.
        if (playerPrefab == null)
        {
            Debug.LogError("Player Prefab n�o est� atribu�do no ServerSceneManager!");
            return;
        }

        GameObject playerInstance = Instantiate(playerPrefab);
        // O SpawnAsPlayerObject associa este objeto ao cliente espec�fico.
        playerInstance.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId, true);
    }

    // --- MUDAN�A 5: Limpeza de Eventos (Boa Pr�tica) ---
    public override void OnNetworkDespawn()
    {
        if (IsServer && NetworkManager.Singleton != null)
        {
            // � importante remover a subscri��o dos eventos quando o objeto � destru�do
            // para evitar erros e memory leaks.
            NetworkManager.Singleton.OnClientConnectedCallback -= HandleClientConnected;
        }
    }
}