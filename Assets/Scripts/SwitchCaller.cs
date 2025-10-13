using System;
using TriInspector;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering;
using UnityEngine.Serialization;

/// TEST CLASS
/// Only use this class for testing.
/// Generic class to set and call any switch in the inspector.
[HideMonoScript]
public class SwitchCaller : MonoBehaviour
{
    [SerializeField] private int switchChannel;
    
    [Button(ButtonSizes.Large, "Call Switch On")]
    private void OnSwitchOn()
    {
        SwitchManager.SetSwitch(switchChannel, true);
    }
    
    [Button(ButtonSizes.Large, "Call Switch Off")]
    private void OnSwitchOff()
    {
        SwitchManager.SetSwitch(switchChannel, false);
    }
}
