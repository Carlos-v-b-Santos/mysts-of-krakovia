using System;

public class PlayerEvents
{
    public event Action<int> OnLevelUp;
    public void LevelUp(int pointsForAttributes)
    {
        OnLevelUp?.Invoke(pointsForAttributes);
    }

    public event Action<int> OnXPReceived;
    public void XPReceived(int amount)
    {
        OnXPReceived?.Invoke(amount);
    }

    public event Action<string> OnAttributeIncreased;
    public void AttributeIncreased(string attribute)
    {
        OnAttributeIncreased?.Invoke(attribute);
    }

    public event Action<int> OnAttributeChanged;
    public void AttributeChanged(int pointsForAttributes)
    {
        OnAttributeChanged?.Invoke(pointsForAttributes);
    }
}
