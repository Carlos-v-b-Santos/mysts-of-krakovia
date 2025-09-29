using UnityEngine;

//[RequireComponent(typeof(CircleCollider2D))]
public class QuestPoint : MonoBehaviour
{
    [Header("Dialogue (optional)")]
    [SerializeField] private TextAsset dialogueFile;

    [Header("Quest")]
    [SerializeField] private QuestInfoSO questInfoForPoint;

    [Header("Config")]
    [SerializeField] private bool startPoint = true;
    [SerializeField] private bool finishPoint = true;

    [Header("Visual Cue")]
    [SerializeField] private GameObject visualCue;

    private bool playerIsNear = false;
    private string questId;
    [SerializeField] private QuestState currentQuestState;

    private QuestIcon questIcon;

    private void Awake()
    {
        questId = questInfoForPoint.Id;
        questIcon = GetComponentInChildren<QuestIcon>();
        visualCue.SetActive(false);
    }

    private void OnEnable()
    {
        GameEventsManager.Instance.questEvents.OnQuestStateChange += QuestStateChange;
        GameEventsManager.Instance.inputEvents.OnInteractPressed += InteractPressed;
    }

    private void OnDisable()
    {
        GameEventsManager.Instance.questEvents.OnQuestStateChange -= QuestStateChange;
        GameEventsManager.Instance.inputEvents.OnInteractPressed -= InteractPressed;
    }

    private void InteractPressed()
    {
        Debug.Log("InteractPressed at QuestPoint: " + questId);

        if (!playerIsNear) //|| DialogueManager.GetInstance().DialogueIsPlaying)
        {
            return;
        }
        
        visualCue.SetActive(false);
        
        // if we have a knot name defined, try to start dialogue with it
        if (!dialogueFile.Equals("")) //&& InputManager.GetInstance().GetInteractPressed())
        {
            DialogueManager.GetInstance().EnterDialogueMode(dialogueFile);
        }
        // otherwise, start or finish the quest immediately without dialogue
        else
        {
            // start or finish a quest
            if (currentQuestState.Equals(QuestState.CAN_START) && startPoint)
            {
                GameEventsManager.Instance.questEvents.StartQuest(questId);
            }
            else if (currentQuestState.Equals(QuestState.CAN_FINISH) && finishPoint)
            {
                GameEventsManager.Instance.questEvents.FinishQuest(questId);
            }
        }
    }

    private void QuestStateChange(Quest quest)
    {
        // only update the quest state if this point has the corresponding quest
        if (quest.info.Id.Equals(questId))
        {
            currentQuestState = quest.state;
            questIcon.SetState(currentQuestState, startPoint, finishPoint);
        }
    }

    private void OnTriggerEnter(Collider otherCollider)
    {
        print("OnTriggerEnter: " + otherCollider.name);
        if (otherCollider.CompareTag("Player"))
        {
            print("Player is near quest point: " + questId);
            playerIsNear = true;
            visualCue.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider otherCollider)
    {
        if (otherCollider.CompareTag("Player"))
        {
            playerIsNear = false;
            visualCue.SetActive(false);
        }
    }
}