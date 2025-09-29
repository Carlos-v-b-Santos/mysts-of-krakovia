using UnityEngine;
using System;

public class InputEvents
{
    public event Action<Vector2> OnMovePressed;
    public void MovePressed(Vector2 moveDir)
    {
        OnMovePressed?.Invoke(moveDir);
    }

    public event Action OnSubmitPressed;
    public void SubmitPressed()
    {
        //if (onSubmitPressed != null)
        //{
        //    onSubmitPressed();
        //}
        // is this the same of this:
        OnSubmitPressed?.Invoke();
    }

    public event Action OnInteractPressed;
    public void InteractPressed()
    {
        OnInteractPressed?.Invoke();
    }

    public event Action OnQuestLogTogglePressed;
    public void QuestLogTogglePressed()
    {
        OnQuestLogTogglePressed?.Invoke();
    }
    
    public event Action<float> OnChangeCameraAngle;
    public void ChangeCameraAngle(float var)
    {
        OnChangeCameraAngle?.Invoke(var);
    }
}