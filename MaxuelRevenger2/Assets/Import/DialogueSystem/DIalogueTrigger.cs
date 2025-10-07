using UnityEngine;

public class DialogueTrigger : MonoBehaviour
{
    [Header("Visual Cue")]
    [SerializeField] private GameObject visualCue;

    [Header("Emote Animator")]
    [SerializeField] private Animator emoteAnimator;

    [Header("Ink JSON")]
    [SerializeField] private TextAsset inkJSON;

    private bool playerInRange;
    
    private void OnEnable()
    {
        GameEventsManager.Instance.inputEvents.OnInteractPressed += InteractPressed;
    }

    private void OnDisable()
    {
        GameEventsManager.Instance.inputEvents.OnInteractPressed -= InteractPressed;
    }

    private void Awake()
    {
        playerInRange = false;
        visualCue.SetActive(false);
    }

    private void Update() 
    {
        if (playerInRange && !DialogueManager.GetInstance().DialogueIsPlaying)
        {
            visualCue.SetActive(true);
        }
        else
        {
            visualCue.SetActive(false);
        }
    }
    
    private void InteractPressed()
    {
        if (playerInRange && !DialogueManager.GetInstance().DialogueIsPlaying)
        {
            DialogueManager.GetInstance().EnterDialogueMode(inkJSON);//, emoteAnimator);
        }
       
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {

        if (collider.gameObject.CompareTag("Player"))
        {
            print("Player entered trigger zone");
            playerInRange = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collider) 
    {
        if (collider.gameObject.CompareTag("Player"))
        {
            print("Player exited trigger zone");
            playerInRange = false;
        }
    }
}