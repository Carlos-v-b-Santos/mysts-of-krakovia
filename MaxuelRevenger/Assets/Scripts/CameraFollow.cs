using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    // O alvo que a câmara deve seguir (o nosso jogador).
    public Transform target;

    // A velocidade de suavização do movimento da câmara.
    // Um valor mais baixo torna o movimento mais suave e lento.
    [SerializeField] private float smoothSpeed = 0.125f;

    // O desvio da câmara em relação ao jogador (permite-nos ajustar o enquadramento).
    [SerializeField] private Vector3 offset;

    // LateUpdate é chamado depois de todos os métodos Update.
    // É o melhor sítio para a lógica da câmara, pois garante que o jogador já se moveu.
    void LateUpdate()
    {
        // Se não tivermos um alvo, não faça nada.
        if (target == null) return;

        // Calcula a posição desejada para a câmara (posição do alvo + desvio).
        Vector3 desiredPosition = target.position + offset;

        // Interpola suavemente da posição atual da câmara para a posição desejada.
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);

        // Aplica a nova posição à câmara.
        transform.position = smoothedPosition;
    }
}