using UnityEngine;
using Unity.Netcode;

public class EnemySpawner : NetworkBehaviour
{
    [Header("Configura��es do Spawner")]
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private int numberOfEnemies = 3;

    [Header("Pontos de Patrulha")]
    [SerializeField] private Transform patrolPointA;
    [SerializeField] private Transform patrolPointB;

    // OnNetworkSpawn n�o ser� mais usado para a l�gica de spawn.
    // Deixamos ele vazio ou podemos at� remov�-lo.
    public override void OnNetworkSpawn()
    {
        // A l�gica foi movida.
    }

    // ++ NOVO M�TODO P�BLICO ++
    // Este m�todo ser� chamado pelo ServerSceneManager quando a cena estiver pronta.
    public void StartSpawning()
    {
        // A l�gica que estava em OnNetworkSpawn agora vive aqui.
        // Como este m�todo s� ser� chamado pelo ServerSceneManager (que j� checa se � servidor),
        // a checagem 'if (!IsServer)' aqui � redundante, mas podemos manter por seguran�a.
        if (!IsServer)
        {
            return;
        }

        if (patrolPointA == null || patrolPointB == null)
        {
            Debug.LogError("Os pontos de patrulha n�o foram atribu�dos no EnemySpawner!", this.gameObject);
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
                Debug.LogError($"O prefab do inimigo '{enemyPrefab.name}' n�o tem o script EnemyController!", enemyInstance);
            }

            enemyInstance.GetComponent<NetworkObject>().Spawn(true);
        }

        Debug.Log($"<color=green>EnemySpawner concluiu o spawn de {numberOfEnemies} inimigos.</color>");
    }
}