using System;
using TriInspector;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering;
using UnityEngine.Serialization;

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
    
    /// <summary>
    /// Call functions added on this behaviour in the Unity Editor.
    /// </summary>
    /// <param name="channel">The channel ID.</param>
    private void OnSwitchCalled(int channel)
    {
        onSwitchCallReceived?.Invoke(channel);
    }
}
