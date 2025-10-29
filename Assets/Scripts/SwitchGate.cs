using System.Collections.Generic;
using TriInspector;
using UnityEngine;

/// Can be placed on any GameObject to act as a logical gate for switches.
/// Takes a list of input channels and sends a signal to the output if conditions are met.
[HideMonoScript]
public class SwitchGate : MonoBehaviour
{
    /// And gate will send the output if all inputs are true.
    /// Or gate will send the output if one of the inputs is true.
    /// Xor gate will send the output if the number or true inputs is odd.
    private enum GateType
    {
        And,
        Or,
        Xor,
    }
    
    /// Switch gate type.
    [Tooltip("Switch gate type.")]
    [OnValueChanged(nameof(UpdateName))]
    [SerializeField] private GateType gateType;
    
    /// Invert this gate's output signal value.
    [Tooltip("Invert this gate's output signal value.")]
    [OnValueChanged(nameof(UpdateName))]
    [SerializeField] private bool inverted;
    
    /// Channels being listened to.
    [Tooltip("Channels being listened to.")]
    [OnValueChanged(nameof(UpdateName))]
    [SerializeField] private List<int> inputChannels;
    
    /// Channel which is being sent a signal to.
    [Tooltip("Channel which is being sent a signal to.")]
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
    
    /// Callback to trigger re-evaluation of the gate's output value.
    /// <param name="channel">The channel ID.</param>
    private void OnSwitchChanged(int channel)
    {
        if (!inputChannels.Contains(channel))
            return;

        var result = false;
        
        switch (gateType)
        {
            case GateType.And:
                result = true;
                foreach (var inputChannel in inputChannels)
                    result &= SwitchManager.GetSwitch(inputChannel);
                break;
            case GateType.Or:
                foreach (var inputChannel in inputChannels)
                    result |= SwitchManager.GetSwitch(inputChannel);
                break;
            case GateType.Xor:
                foreach (var inputChannel in inputChannels)
                    result ^= SwitchManager.GetSwitch(inputChannel);
                break;
        }
        
        result ^= inverted;
        SwitchManager.SetSwitch(outputChannel, result);
    }
    
    /// Internal function to rename the gate's GameObject automatically when properties are changed.
    private void UpdateName()
    {
        var newName = inverted ? (gateType == GateType.Xor ? "Xnor" : "N" + gateType.ToString().ToLower()) : gateType.ToString();
        newName += "Gate_";
        foreach (var inputChannel in inputChannels)
            newName += inputChannel + "-";
        newName = newName.TrimEnd('-');
        newName += "_" + outputChannel;
        this.name = newName;
    }
}
