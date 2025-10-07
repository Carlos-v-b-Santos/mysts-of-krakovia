using UnityEngine;
using Unity.Netcode; // Importe o Netcode se precisar de lógica de rede aqui (opcional)

public class DialogueTrigger : MonoBehaviour
{
    [Header("Visual Cue")]
    [SerializeField] private GameObject visualCue;

    [Header("Ink JSON")]
    [SerializeField] private TextAsset inkJSON;

    private bool playerInRange;

    // Guarda a referência para o input do jogador que está na área
    private PlayerInput playerInputInRange;

    private void Awake()
    {
        playerInRange = false;
        visualCue.SetActive(false);
    }

    private void Update()
    {
        // A lógica para mostrar a dica visual continua a mesma
        if (playerInRange && !DialogueManager.GetInstance().DialogueIsPlaying)
        {
            visualCue.SetActive(true);
        }
        else
        {
            visualCue.SetActive(false);
        }
    }

    // Chamado quando o evento OnInteractPressed do nosso jogador local é disparado
    private void InteractPressed()
    {
        // Se o jogador estiver na área e o diálogo não estiver a decorrer, inicia o diálogo
        if (playerInRange && !DialogueManager.GetInstance().DialogueIsPlaying)
        {
            DialogueManager.GetInstance().EnterDialogueMode(inkJSON);
        }
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        // Verifica se o objeto que entrou é um jogador
        if (collider.gameObject.CompareTag("Player"))
        {
            // Tenta obter o componente PlayerInput
            PlayerInput playerInput = collider.GetComponent<PlayerInput>();
            // Apenas o jogador local (que tem o componente PlayerInput ativo) deve poder interagir
            if (playerInput != null && playerInput.enabled)
            {
                playerInRange = true;
                playerInputInRange = playerInput;
                // Inscreve-se no evento de interação DESTE jogador específico
                playerInputInRange.OnInteractPressed += InteractPressed;
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collider)
    {
        if (collider.gameObject.CompareTag("Player"))
        {
            // Se o jogador que está a sair é o mesmo que estava na área, limpa tudo
            if (collider.GetComponent<PlayerInput>() == playerInputInRange)
            {
                playerInRange = false;
                // Desinscreve-se do evento para parar de "ouvir"
                playerInputInRange.OnInteractPressed -= InteractPressed;
                playerInputInRange = null;
            }
        }
    }
}