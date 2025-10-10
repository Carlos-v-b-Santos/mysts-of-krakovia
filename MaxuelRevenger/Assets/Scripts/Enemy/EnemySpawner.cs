using UnityEngine;
using Unity.Netcode;
using System;

public class EnemySpawner : NetworkBehaviour
{
    [Header("Configura��es do Spawner")]
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private int numberOfEnemies = 3;

    [Header("Pontos de Patrulha")]
    [SerializeField] private Transform patrolPointA;
    [SerializeField] private Transform patrolPointB;

    private void Awake()
    {
        // Este log foi �til para o debug, pode ser mantido ou removido.
        Debug.Log("[EnemySpawner] Awake: O componente est� sendo inicializado pela Unity.");
    }

    public void StartSpawning()
    {
        Debug.Log("[EnemySpawner] M�todo StartSpawning() FOI CHAMADO.");

        // A VERIFICA��O PROBLEM�TICA FOI REMOVIDA DAQUI
        // A autoridade do servidor j� � garantida pelo ServerSceneManager, que � o �nico que chama este m�todo.

        if (enemyPrefab == null)
        {
            Debug.LogError("[EnemySpawner] FALHA NO SPAWN: O campo 'Enemy Prefab' est� VAZIO (None) no Inspector.");
            return;
        }

        if (patrolPointA == null || patrolPointB == null)
        {
            Debug.LogError("[EnemySpawner] FALHA NO SPAWN: Os campos 'Patrol Point A' ou 'Patrol Point B' est�o VAZIOS (None) no Inspector.");
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
                Debug.LogError($"[EnemySpawner] ERRO CR�TICO DURANTE A INSTANCIA��O do prefab '{enemyPrefab.name}'. Erro: {e.Message}");
                return;
            }
        }

        Debug.Log($"<color=green>[EnemySpawner] PROCESSO DE SPAWN CONCLU�DO. {numberOfEnemies} inimigos criados.</color>");
    }
}