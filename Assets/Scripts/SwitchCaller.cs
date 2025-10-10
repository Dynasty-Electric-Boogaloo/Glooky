using System;
using TriInspector;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering;
using UnityEngine.Serialization;

[HideMonoScript]
public class SwitchCaller : MonoBehaviour
{
    [ListDrawerSettings(AlwaysExpanded = true)]
    public uint[] switchAt;
    
    [Button(ButtonSizes.Large, "Call Switch On")]
    private void OnSwitchOn()
    {
        SwitchManager.Instance.SwitchOnAt(switchAt);
    }
    
    [Button(ButtonSizes.Large, "Call Switch Off")]
    private void OnSwitchOff()
    {
        SwitchManager.Instance.SwitchOffAt(switchAt);
    }
}
