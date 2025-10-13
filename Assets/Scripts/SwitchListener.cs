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

    private void OnSwitchCalled(int value)
    {
        onSwitchCallReceived?.Invoke(value);
    }
}
