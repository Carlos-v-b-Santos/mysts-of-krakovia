using UnityEngine;
using Ink.Runtime;
using UnityEngine.InputSystem.Interactions;

public class InkExternalFunctions
{
    public void Bind(Story story)//, Animator emoteAnimator)
    {
        //story.BindExternalFunction("playEmote", (string emoteName) => PlayEmote(emoteName, emoteAnimator));

        story.BindExternalFunction("StartQuest", (string questId) => StartQuest(questId));
        story.BindExternalFunction("AdvanceQuest", (string questId) => AdvanceQuest(questId));
        story.BindExternalFunction("FinishQuest", (string questId) => FinishQuest(questId));

        Debug.Log("Bind");
    }

    public void Unbind(Story story)
    {
        //story.UnbindExternalFunction("playEmote");

        story.UnbindExternalFunction("StartQuest");
        story.UnbindExternalFunction("AdvanceQuest");
        story.UnbindExternalFunction("FinishQuest");

        Debug.Log("Unbind");
    }

    public void PlayEmote(string emoteName, Animator emoteAnimator)
    {
        if (emoteAnimator != null)
        {
            emoteAnimator.Play(emoteName);
        }
        else
        {
            Debug.LogWarning("Tried to play emote, but emote animator was "
                + "not initialized when entering dialogue mode.");
        }
    }
    
    
    private void StartQuest(string questId) 
    {
        GameEventsManager.Instance.questEvents.StartQuest(questId);
    }

    private void AdvanceQuest(string questId) 
    {
        GameEventsManager.Instance.questEvents.AdvanceQuest(questId);
    }

    private void FinishQuest(string questId)
    {
        GameEventsManager.Instance.questEvents.FinishQuest(questId);
    }
}