using System;
using UnityEngine;
using TriInspector;

/// TEST CLASS
/// Only use this class for testing.
/// Generic class to set and call any switch in the inspector.
[HideMonoScript]
public class SwitchCaller : MonoBehaviour
{
    [SerializeField] private int outputChannel;
    
    [Button(ButtonSizes.Large, "Call Switch On")]
    private void OnSwitchOn()
    {
        SwitchManager.SetSwitch(outputChannel, true);
    }
    
    [Button(ButtonSizes.Large, "Call Switch Off")]
    private void OnSwitchOff()
    {
        SwitchManager.SetSwitch(outputChannel, false);
    }
}
