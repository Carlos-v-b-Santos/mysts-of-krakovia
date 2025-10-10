using UnityEngine;
using Cinemachine; // Importe o namespace do Cinemachine

// Este script encontra o jogador local e diz � C�mara Virtual para o seguir.
[RequireComponent(typeof(CinemachineVirtualCamera))]
public class CinemachineTargetAssigner : MonoBehaviour
{
    private CinemachineVirtualCamera virtualCamera;
    private bool targetAssigned = false;

    void Awake()
    {
        virtualCamera = GetComponent<CinemachineVirtualCamera>();
    }

    // Usamos LateUpdate para garantir que o jogador j� foi criado na rede (spawned).
    void LateUpdate()
    {
        // Se j� atribu�mos o alvo, n�o precisamos de fazer mais nada.
        if (targetAssigned)
        {
            // Opcional: Desativa este script para otimiza��o depois de encontrar o alvo.
            // this.enabled = false; 
            return;
        }

        // Procura pela inst�ncia do jogador local que o nosso PlayerController define.
        if (PlayerController.LocalInstance != null)
        {
            Debug.Log("Alvo da c�mara encontrado! A seguir o jogador local.");

            // Diz � c�mara virtual para seguir e olhar para o nosso jogador.
            Transform playerTransform = PlayerController.LocalInstance.transform;
            virtualCamera.Follow = playerTransform;
            virtualCamera.LookAt = playerTransform; // Opcional, mas geralmente desejado.

            targetAssigned = true;
        }
    }
}