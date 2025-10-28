using System.Collections.Generic;
using TriInspector;
using UnityEngine;

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
    [SerializeField] private bool inverted;
    
    [SerializeField] private List<int> inputChannels;
    
    [SerializeField] private int outputChannel;

    private void OnEnable()
    {
        foreach (var inputChannel in inputChannels)
            SwitchManager.AddListenerOnChannel(OnSwitchChanged, inputChannel);
    }

    private void OnDisable()
    {
        foreach (var inputChannel in inputChannels)
            SwitchManager.RemoveListenerOnChannel(OnSwitchChanged, inputChannel);
    }
    
    /// Execute behaviour.
    /// <param name="channel">The channel ID.</param>
    private void OnSwitchChanged(int channel)
    {
        if (!inputChannels.Contains(channel))
            return;

        var result = false;
        
        switch (type)
        {
            case Type.And:
                result = true;
                foreach (var inputChannel in inputChannels)
                    result &= SwitchManager.GetSwitch(inputChannel);
                break;
            case Type.Or:
                foreach (var inputChannel in inputChannels)
                    result |= SwitchManager.GetSwitch(inputChannel);
                break;
            case Type.Xor:
                foreach (var inputChannel in inputChannels)
                    result ^= SwitchManager.GetSwitch(inputChannel);
                break;
        }
        
        result ^= inverted;
        SwitchManager.SetSwitch(outputChannel, result);
    }
}
