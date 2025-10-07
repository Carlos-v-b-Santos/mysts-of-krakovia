using UnityEngine;

public class ParallaxEffect : MonoBehaviour
{
    // Referência para a câmara principal do jogo.
    private Transform cameraTransform;

    // Guarda a posição inicial da camada e da câmara.
    private Vector3 lastCameraPosition;

    // Fator que determina o quão rápido esta camada se move.
    // Valores mais próximos de 1 fazem com que se mova muito pouco (fundo distante).
    // Valores mais próximos de 0 fazem com que se mova quase junto com a câmara (frente).
    [SerializeField][Range(0f, 1f)] private float parallaxEffectMultiplier;

    void Start()
    {
        // Encontra a câmara principal e guarda as posições iniciais.
        cameraTransform = Camera.main.transform;
        lastCameraPosition = cameraTransform.position;
    }

    // LateUpdate é chamado depois de todos os outros Updates, garantindo que a câmara já se moveu.
    void LateUpdate()
    {
        // Calcula o quanto a câmara se moveu desde o último frame.
        float deltaX = cameraTransform.position.x - lastCameraPosition.x;

        // Move esta camada na mesma direção, mas multiplicado pelo nosso fator de parallax.
        transform.position += Vector3.right * deltaX * parallaxEffectMultiplier;

        // Atualiza a última posição da câmara para o próximo frame.
        lastCameraPosition = cameraTransform.position;
    }
}