using UnityEngine;
using System;

public class InputEvents
{
    public event Action<float> OnMovePressed;
    public void MovePressed(float moveDir)
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

    public event Action OnJumpPressed;
    public void JumpPressed()
    {
        OnJumpPressed?.Invoke();
    }

    public event Action OnShootPressed;
    public void ShootPressed()
    {
        OnShootPressed?.Invoke();
    }

    public event Action OnShootReleased;
    public void ShootReleased()
    {
        OnShootReleased?.Invoke();
    }

    public event Action OnAttackPressed;
    public void AttackPressed()
    {
        OnAttackPressed?.Invoke();
    }
}