using UnityEngine;
using Unity.Netcode;
using System;

public class EnemySpawner : NetworkBehaviour
{
    [Header("Configurações do Spawner")]
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private int numberOfEnemies = 3;

    [Header("Pontos de Patrulha")]
    [SerializeField] private Transform patrolPointA;
    [SerializeField] private Transform patrolPointB;

    private void Awake()
    {
        // Este log foi útil para o debug, pode ser mantido ou removido.
        Debug.Log("[EnemySpawner] Awake: O componente está sendo inicializado pela Unity.");
    }

    public void StartSpawning()
    {
        Debug.Log("[EnemySpawner] Método StartSpawning() FOI CHAMADO.");

        // A VERIFICAÇÃO PROBLEMÁTICA FOI REMOVIDA DAQUI
        // A autoridade do servidor já é garantida pelo ServerSceneManager, que é o único que chama este método.

        if (enemyPrefab == null)
        {
            Debug.LogError("[EnemySpawner] FALHA NO SPAWN: O campo 'Enemy Prefab' está VAZIO (None) no Inspector.");
            return;
        }

        if (patrolPointA == null || patrolPointB == null)
        {
            Debug.LogError("[EnemySpawner] FALHA NO SPAWN: Os campos 'Patrol Point A' ou 'Patrol Point B' estão VAZIOS (None) no Inspector.");
            return;
        }

        for (int i = 0; i < numberOfEnemies; i++)
        {
            try
            {
                GameObject enemyInstance = Instantiate(enemyPrefab, transform.position, Quaternion.identity);
                EnemyController enemyController = enemyInstance.GetComponent<EnemyController>();
                if (enemyController != null)
                {
                    enemyController.SetPatrolPoints(patrolPointA, patrolPointB);
                }

                NetworkObject netObj = enemyInstance.GetComponent<NetworkObject>();
                netObj.Spawn(true);
            }
            catch (Exception e)
            {
                Debug.LogError($"[EnemySpawner] ERRO CRÍTICO DURANTE A INSTANCIAÇÃO do prefab '{enemyPrefab.name}'. Erro: {e.Message}");
                return;
            }
        }

        Debug.Log($"<color=green>[EnemySpawner] PROCESSO DE SPAWN CONCLUÍDO. {numberOfEnemies} inimigos criados.</color>");
    }
}