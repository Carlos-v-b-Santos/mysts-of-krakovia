using UnityEngine;
using System.Collections;

// Este script � respons�vel por todos os feedbacks visuais e de �udio do jogador.
[RequireComponent(typeof(PlayerHealth))]
[RequireComponent(typeof(SpriteRenderer))]
public class PlayerFeedback : MonoBehaviour
{
    private PlayerHealth playerHealth;
    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        playerHealth = GetComponent<PlayerHealth>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void OnEnable()
    {
        // Inscreve-se no evento OnHurt. Agora, quando a vida mudar, o m�todo HandleHurt ser� chamado.
        playerHealth.OnHurt += HandleHurt;
    }

    private void OnDisable()
    {
        // � importante desinscrever-se para evitar erros.
        playerHealth.OnHurt -= HandleHurt;
    }

    // Este m�todo � chamado pelo evento OnHurt do PlayerHealth.
    private void HandleHurt(Transform attacker)
    {
        // Inicia a corrotina que faz o sprite piscar em vermelho.
        StartCoroutine(DamageFlashCoroutine());

        // Futuramente, voc� pode adicionar aqui:
        // - Chamada para o UIManager atualizar a barra de vida.
        // - Chamada para um AudioManager tocar um som de dor.
    }

    private IEnumerator DamageFlashCoroutine()
    {
        spriteRenderer.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        spriteRenderer.color = Color.white;
        yield return new WaitForSeconds(0.1f);
        spriteRenderer.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        spriteRenderer.color = Color.white;
    }
}