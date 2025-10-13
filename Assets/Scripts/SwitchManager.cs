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
    
    public UnityEvent<int>[] onSwitchCalled = new UnityEvent<int>[128];
    
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
    
    public static void AddListenerOnChannel(UnityAction<int> call, int channel)
    {
        if (_instance == null)
        {
            Debug.LogWarning($"SwitchManager instance is null (AddListenerOnChannel)");
            return;
        }
        
        if (channel >= _instance.onSwitchCalled.Length || channel < 0)
        {
            Debug.LogWarning($"Invalid channel {channel} (AddListenerOnChannel)");
            return;
        }

        _instance.onSwitchCalled[channel].AddListener(call);
    }
    
    public static void RemoveListenerOnChannel(UnityAction<int> call, int channel)
    {
        if (_instance == null)
        {
            Debug.LogWarning($"SwitchManager instance is null (RemoveListenerOnChannel)");
            return;
        }

        if (channel >= _instance.onSwitchCalled.Length || channel < 0)
        {
            Debug.LogWarning($"Invalid channel {channel} (RemoveListenerOnChannel)");
            return;
        }
            

        _instance.onSwitchCalled[channel].RemoveListener(call);
    }
    
    public static bool GetSwitch(int channel)
    {
        if (_instance == null)
        {
            Debug.LogWarning($"SwitchManager instance is null (GetSwitch)");
            return false;
        }

        if (channel >= _instance.onSwitchCalled.Length || channel < 0)
        {
            Debug.LogWarning($"Invalid channel {channel} (GetSwitch)");
            return false;
        }
        
        var uChannel = (uint) channel;
        return _instance._switches[uChannel];
    }
    
    public static void SetSwitch(int channel, bool value)
    {
        if (_instance == null)
        {
            Debug.LogWarning($"SwitchManager instance is null (SetSwitch)");
            return;
        }

        if (channel >= _instance.onSwitchCalled.Length || channel < 0)
        {
            Debug.LogWarning($"Invalid channel {channel} (SetSwitch)");
            return;
        }
        
        var uChannel = (uint) channel;
        _instance._switches[uChannel] = value;
        _instance.onSwitchCalled[channel]?.Invoke(channel);
    }
    
    public static void ResetSwitches()
    {
        if (_instance == null)
        {
            Debug.LogWarning($"SwitchManager instance is null (ResetSwitches)");
            return;
        }
        
        _instance._switches = new BitArray128();
    }
}
