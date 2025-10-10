using UnityEngine;
using Unity.Netcode;

public class EnemySpawner : NetworkBehaviour
{
    [Header("Configurações do Spawner")]
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private int numberOfEnemies = 3;

    [Header("Pontos de Patrulha")]
    [SerializeField] private Transform patrolPointA;
    [SerializeField] private Transform patrolPointB;

    // OnNetworkSpawn não será mais usado para a lógica de spawn.
    // Deixamos ele vazio ou podemos até removê-lo.
    public override void OnNetworkSpawn()
    {
        // A lógica foi movida.
    }

    // ++ NOVO MÉTODO PÚBLICO ++
    // Este método será chamado pelo ServerSceneManager quando a cena estiver pronta.
    public void StartSpawning()
    {
        // A lógica que estava em OnNetworkSpawn agora vive aqui.
        // Como este método só será chamado pelo ServerSceneManager (que já checa se é servidor),
        // a checagem 'if (!IsServer)' aqui é redundante, mas podemos manter por segurança.
        if (!IsServer)
        {
            return;
        }

        if (patrolPointA == null || patrolPointB == null)
        {
            Debug.LogError("Os pontos de patrulha não foram atribuídos no EnemySpawner!", this.gameObject);
            return;
        }

        for (int i = 0; i < numberOfEnemies; i++)
        {
            GameObject enemyInstance = Instantiate(enemyPrefab, transform.position, Quaternion.identity);

            EnemyController enemyController = enemyInstance.GetComponent<EnemyController>();
            if (enemyController != null)
            {
                enemyController.SetPatrolPoints(patrolPointA, patrolPointB);
            }
            else
            {
                Debug.LogError($"O prefab do inimigo '{enemyPrefab.name}' não tem o script EnemyController!", enemyInstance);
            }

            enemyInstance.GetComponent<NetworkObject>().Spawn(true);
        }

        Debug.Log($"<color=green>EnemySpawner concluiu o spawn de {numberOfEnemies} inimigos.</color>");
    }
}