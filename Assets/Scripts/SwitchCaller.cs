using System;
using TriInspector;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering;
using UnityEngine.Serialization;

// To be used for testing
[HideMonoScript]
public class SwitchCaller : MonoBehaviour
{
    public uint switchAt;
    
    [Button(ButtonSizes.Large, "Call Switch On")]
    private void OnSwitchOn()
    {
        SwitchManager.Instance.SetSwitch(switchAt, true);
    }
    
    [Button(ButtonSizes.Large, "Call Switch Off")]
    private void OnSwitchOff()
    {
        SwitchManager.Instance.SetSwitch(switchAt, false);
    }
}
