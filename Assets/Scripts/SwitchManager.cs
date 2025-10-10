using System;
using TriInspector;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering;
using UnityEngine.Serialization;

[HideMonoScript]
public class SwitchManager : MonoBehaviour
{
    private static SwitchManager _instance;
    public static SwitchManager Instance => _instance;
    
    public BitArray128 globalSwitch = new();

    public UnityEvent<BitArray128> onSwitchCalled = new();
    
    private void Awake()
    {
        if (_instance != null)
        {
            Destroy(this);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(_instance);
    }
    
    public void SwitchOnAt(params uint[] indices)
    {
        foreach (var i in indices)
            globalSwitch[i] = true;

        onSwitchCalled?.Invoke(globalSwitch);
    }
    
    public void SwitchOffAt(params uint[] indices)
    {
        foreach (var i in indices)
            globalSwitch[i] = false;

        onSwitchCalled?.Invoke(globalSwitch);
    }
    
    public void ResetGlobalSwitch()
    {
        globalSwitch = new BitArray128();
    }
}
