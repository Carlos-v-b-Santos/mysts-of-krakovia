using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerEvents : MonoBehaviour
{
    public event Action<int> OnLevelUp;
    public event Action<int> OnXPReceived;
    public event Action<string> OnAttributeIncreased;
    public event Action<int> OnAttributeChanged;

    public void LevelUp(int pointsForAttributes) => OnLevelUp?.Invoke(pointsForAttributes);
    public void XPReceived(int amount) => OnXPReceived?.Invoke(amount);
    public void AttributeIncreased(string attribute) => OnAttributeIncreased?.Invoke(attribute);
    public void AttributeChanged(int pointsForAttributes) => OnAttributeChanged?.Invoke(pointsForAttributes);
}
