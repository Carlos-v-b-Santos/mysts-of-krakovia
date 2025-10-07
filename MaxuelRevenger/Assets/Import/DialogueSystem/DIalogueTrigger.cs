using UnityEngine;
using Unity.Netcode; // Importe o Netcode se precisar de l�gica de rede aqui (opcional)

public class DialogueTrigger : MonoBehaviour
{
    [Header("Visual Cue")]
    [SerializeField] private GameObject visualCue;

    [Header("Ink JSON")]
    [SerializeField] private TextAsset inkJSON;

    private bool playerInRange;

    // Guarda a refer�ncia para o input do jogador que est� na �rea
    private PlayerInput playerInputInRange;

    private void Awake()
    {
        playerInRange = false;
        visualCue.SetActive(false);
    }

    private void Update()
    {
        // A l�gica para mostrar a dica visual continua a mesma
        if (playerInRange && !DialogueManager.GetInstance().DialogueIsPlaying)
        {
            visualCue.SetActive(true);
        }
        else
        {
            visualCue.SetActive(false);
        }
    }

    // Chamado quando o evento OnInteractPressed do nosso jogador local � disparado
    private void InteractPressed()
    {
        // Se o jogador estiver na �rea e o di�logo n�o estiver a decorrer, inicia o di�logo
        if (playerInRange && !DialogueManager.GetInstance().DialogueIsPlaying)
        {
            DialogueManager.GetInstance().EnterDialogueMode(inkJSON);
        }
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        // Verifica se o objeto que entrou � um jogador
        if (collider.gameObject.CompareTag("Player"))
        {
            // Tenta obter o componente PlayerInput
            PlayerInput playerInput = collider.GetComponent<PlayerInput>();
            // Apenas o jogador local (que tem o componente PlayerInput ativo) deve poder interagir
            if (playerInput != null && playerInput.enabled)
            {
                playerInRange = true;
                playerInputInRange = playerInput;
                // Inscreve-se no evento de intera��o DESTE jogador espec�fico
                playerInputInRange.OnInteractPressed += InteractPressed;
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collider)
    {
        if (collider.gameObject.CompareTag("Player"))
        {
            // Se o jogador que est� a sair � o mesmo que estava na �rea, limpa tudo
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