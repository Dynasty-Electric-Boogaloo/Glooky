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
    
    private BitArray128 _switches = new();

    public UnityEvent<bool>[] onSwitchCalled = new UnityEvent<bool>[128];
    
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

    public static void AddListenerOnChannel(UnityAction<bool> call, uint channel)
    {
        if (channel >= _instance.onSwitchCalled.Length)
        {
            Debug.LogWarning($"Invalid channel {channel}");
            return;
        }

        _instance.onSwitchCalled[channel].AddListener(call);
    }
    
    public static void RemoveListenerOnChannel(UnityAction<bool> call, uint channel)
    {
        if (channel >= _instance.onSwitchCalled.Length)
            return;

        _instance.onSwitchCalled[channel].RemoveListener(call);
    }
    
    public bool GetSwitch(uint index)
    {
        return _switches[index];
    }
    
    public void SetSwitch(uint index, bool value)
    {
        _switches[index] = value;

        if (index < onSwitchCalled.Length)
            onSwitchCalled[index]?.Invoke(value);
    }
    
    public void ResetSwitches()
    {
        _switches = new BitArray128();
    }
}