using System;
using TriInspector;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering;
using UnityEngine.Serialization;

[HideMonoScript]
public class SwitchListener : MonoBehaviour
{
    public UnityEvent<bool> onSwitchCallReceived;
    
    [SerializeField] private uint channelListenedTo;

    private void OnEnable()
    {
        SwitchManager.AddListenerOnChannel(OnSwitchCalled, channelListenedTo);
    }

    private void OnDisable()
    {
        SwitchManager.RemoveListenerOnChannel(OnSwitchCalled, channelListenedTo);
    }

    private void OnSwitchCalled(bool value)
    {
        onSwitchCallReceived?.Invoke(value);
    }
}
