using System;

public class PlayerEvents
{
    public event Action OnLevelUp;
    public void LevelUp()
    {
        OnLevelUp?.Invoke();
    }

    public event Action<int> OnXPReceived;
    public void XPReceived(int amount)
    {
        OnXPReceived?.Invoke(amount);
    }
}
