using System;
using TriInspector;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering;
using UnityEngine.Serialization;

/// Generic class to receive switch calls and handle them.
/// Prioritize implementing listeners directly in relevant classes instead.
[HideMonoScript]
public class SwitchListener : MonoBehaviour
{
    public UnityEvent<int> onSwitchChanged;
    
    [SerializeField] private int inputChannel;

    private void OnEnable()
    {
        SwitchManager.AddListenerOnChannel(OnSwitchChanged, inputChannel);
    }

    private void OnDisable()
    {
        SwitchManager.RemoveListenerOnChannel(OnSwitchChanged, inputChannel);
    }
    
    /// Trigger the registered callbacks of the onSwitchChanged event.
    /// <param name="channel">The channel ID.</param>
    private void OnSwitchChanged(int channel)
    {
        onSwitchChanged?.Invoke(channel);
    }
}
