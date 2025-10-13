using System;
using TriInspector;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering;
using UnityEngine.Serialization;

[HideMonoScript]
public class SwitchGate : MonoBehaviour
{
    private enum Type
    {
        And,
        Or,
        Xor,
    }
    
    [SerializeField] private Type type;
    
    [SerializeField] private int channelListenedTo1;
    [SerializeField] private int channelListenedTo2;
    
    [SerializeField] private int switchChannel;

    private void OnEnable()
    {
        SwitchManager.AddListenerOnChannel(OnSwitchChanged, channelListenedTo1);
        SwitchManager.AddListenerOnChannel(OnSwitchChanged, channelListenedTo2);
    }

    private void OnDisable()
    {
        SwitchManager.RemoveListenerOnChannel(OnSwitchChanged, channelListenedTo1);
        SwitchManager.RemoveListenerOnChannel(OnSwitchChanged, channelListenedTo2);
    }
    
    /// Execute behaviour.
    /// <param name="channel">The channel ID.</param>
    private void OnSwitchChanged(int channel)
    {
        if (channel != channelListenedTo1 && channel != channelListenedTo2)
            return;
        
        switch (type)
        {
            case Type.And:
                SwitchManager.SetSwitch(switchChannel, SwitchManager.GetSwitch(channelListenedTo1) & SwitchManager.GetSwitch(channelListenedTo2));
                break;
            case Type.Or:
                SwitchManager.SetSwitch(switchChannel, SwitchManager.GetSwitch(channelListenedTo1) | SwitchManager.GetSwitch(channelListenedTo2));
                break;
            case Type.Xor:
                SwitchManager.SetSwitch(switchChannel, SwitchManager.GetSwitch(channelListenedTo1) ^ SwitchManager.GetSwitch(channelListenedTo2));
                break;
        }
    }
}
