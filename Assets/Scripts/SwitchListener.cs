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
    public UnityEvent<int> onSwitchCallReceived;
    
    [SerializeField] private int channelListenedTo;

    private void OnEnable()
    {
        SwitchManager.AddListenerOnChannel(OnSwitchCalled, channelListenedTo);
    }

    private void OnDisable()
    {
        SwitchManager.RemoveListenerOnChannel(OnSwitchCalled, channelListenedTo);
    }
    
    /// Call functions added on this behaviour in the Unity Editor.
    /// <param name="channel">The channel ID.</param>
    private void OnSwitchCalled(int channel)
    {
        onSwitchCallReceived?.Invoke(channel);
    }
}
