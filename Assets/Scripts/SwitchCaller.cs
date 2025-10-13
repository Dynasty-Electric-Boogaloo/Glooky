using System;
using TriInspector;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering;
using UnityEngine.Serialization;

/// TEST CLASS
[HideMonoScript]
public class SwitchCaller : MonoBehaviour
{
    [SerializeField] private int switchAt;
    
    [Button(ButtonSizes.Large, "Call Switch On")]
    private void OnSwitchOn()
    {
        SwitchManager.SetSwitch(switchAt, true);
    }
    
    [Button(ButtonSizes.Large, "Call Switch Off")]
    private void OnSwitchOff()
    {
        SwitchManager.SetSwitch(switchAt, false);
    }
}
