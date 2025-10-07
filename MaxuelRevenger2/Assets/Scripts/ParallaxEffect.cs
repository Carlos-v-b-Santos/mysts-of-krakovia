using UnityEngine;

public class ParallaxEffect : MonoBehaviour
{
    // Refer�ncia para a c�mara principal do jogo.
    private Transform cameraTransform;

    // Guarda a posi��o inicial da camada e da c�mara.
    private Vector3 lastCameraPosition;

    // Fator que determina o qu�o r�pido esta camada se move.
    // Valores mais pr�ximos de 1 fazem com que se mova muito pouco (fundo distante).
    // Valores mais pr�ximos de 0 fazem com que se mova quase junto com a c�mara (frente).
    [SerializeField][Range(0f, 1f)] private float parallaxEffectMultiplier;

    void Start()
    {
        // Encontra a c�mara principal e guarda as posi��es iniciais.
        cameraTransform = Camera.main.transform;
        lastCameraPosition = cameraTransform.position;
    }

    // LateUpdate � chamado depois de todos os outros Updates, garantindo que a c�mara j� se moveu.
    void LateUpdate()
    {
        // Calcula o quanto a c�mara se moveu desde o �ltimo frame.
        float deltaX = cameraTransform.position.x - lastCameraPosition.x;

        // Move esta camada na mesma dire��o, mas multiplicado pelo nosso fator de parallax.
        transform.position += Vector3.right * deltaX * parallaxEffectMultiplier;

        // Atualiza a �ltima posi��o da c�mara para o pr�ximo frame.
        lastCameraPosition = cameraTransform.position;
    }
}