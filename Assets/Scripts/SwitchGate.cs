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
    
    /// Switch gate type.
    [OnValueChanged(nameof(UpdateName))]
    [SerializeField] private Type type;
    /// Make this gate output the signal when condition is not matched.
    [OnValueChanged(nameof(UpdateName))]
    [SerializeField] private bool inverted;
    
    /// Channels being listened to.
    [OnValueChanged(nameof(UpdateName))]
    [SerializeField] private List<int> inputChannels;
    
    /// Channel which is being sent a signal to.
    [OnValueChanged(nameof(UpdateName))]
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
    
    /// Internal function to rename the gate automatically when properties are changed.
    private void UpdateName()
    {
        var newName = inverted ? (type == Type.Xor ? "Xnor" : "N" + type.ToString().ToLower()) : type.ToString();
        newName += "Gate_";
        foreach (var inputChannel in inputChannels)
            newName += inputChannel + "-";
        newName = newName.TrimEnd('-');
        newName += "_" + outputChannel;
        this.name = newName;
    }
}
