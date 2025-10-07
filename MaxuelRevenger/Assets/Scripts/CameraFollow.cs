using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    // O alvo que a c�mara deve seguir (o nosso jogador).
    public Transform target;

    // A velocidade de suaviza��o do movimento da c�mara.
    // Um valor mais baixo torna o movimento mais suave e lento.
    [SerializeField] private float smoothSpeed = 0.125f;

    // O desvio da c�mara em rela��o ao jogador (permite-nos ajustar o enquadramento).
    [SerializeField] private Vector3 offset;

    // LateUpdate � chamado depois de todos os m�todos Update.
    // � o melhor s�tio para a l�gica da c�mara, pois garante que o jogador j� se moveu.
    void FixedUpdate()
    {
        // Se n�o tivermos um alvo, n�o fa�a nada.
        if (target == null) return;

        // Calcula a posi��o desejada para a c�mara (posi��o do alvo + desvio).
        Vector3 desiredPosition = target.position + offset;

        // Interpola suavemente da posi��o atual da c�mara para a posi��o desejada.
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);

        // Aplica a nova posi��o � c�mara.
        transform.position = smoothedPosition;
    }
}