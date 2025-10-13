using System;
using TriInspector;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering;
using UnityEngine.Serialization;

/// Singleton component that holds the switch array and handles its calls.
[HideMonoScript]
public class SwitchManager : MonoBehaviour
{
    private static SwitchManager _instance;
    
    private BitArray128 _switches = new();
    
    [SerializeField] private UnityEvent<int>[] onSwitchChanged = new UnityEvent<int>[128];
    
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
    
    /// Set a callback to start listening for changes to the switch at the specified channel identifier.
    /// <param name="call">Function called when this channel calls.</param>
    /// <param name="channel">Channel to start listening to.</param>
    public static void AddListenerOnChannel(UnityAction<int> call, int channel)
    {
        if (_instance == null)
        {
            Debug.LogWarning($"SwitchManager instance is null (AddListenerOnChannel)");
            return;
        }
        
        if (channel >= _instance.onSwitchChanged.Length || channel < 0)
        {
            Debug.LogWarning($"Invalid channel {channel} (AddListenerOnChannel)");
            return;
        }

        _instance.onSwitchChanged[channel].AddListener(call);
    }
    
    /// Set a callback to stop listening for changes to the switch at the specified channel identifier.
    /// <param name="call">Function called when this channel calls.</param>
    /// <param name="channel">Channel to stop listening to.</param>
    public static void RemoveListenerOnChannel(UnityAction<int> call, int channel)
    {
        if (_instance == null)
        {
            Debug.LogWarning($"SwitchManager instance is null (RemoveListenerOnChannel)");
            return;
        }

        if (channel >= _instance.onSwitchChanged.Length || channel < 0)
        {
            Debug.LogWarning($"Invalid channel {channel} (RemoveListenerOnChannel)");
            return;
        }
            
        _instance.onSwitchChanged[channel].RemoveListener(call);
    }
    
    /// Get switch's state using specified channel identifier.
    /// <param name="channel">The channel state to get.</param>
    /// <returns>The channel's state.</returns>
    public static bool GetSwitch(int channel)
    {
        if (_instance == null)
        {
            Debug.LogWarning($"SwitchManager instance is null (GetSwitch)");
            return false;
        }

        if (channel >= _instance.onSwitchChanged.Length || channel < 0)
        {
            Debug.LogWarning($"Invalid channel {channel} (GetSwitch)");
            return false;
        }
        
        return _instance._switches[(uint)channel];
    }
    
    /// Set switch's state using specified channel identifier. Invokes onSwitchChanged if the switch state changes.
    /// <param name="channel">Switch to be set.</param>
    /// <param name="value">New Switch value.</param>
    public static void SetSwitch(int channel, bool value)
    {
        if (_instance == null)
        {
            Debug.LogWarning($"SwitchManager instance is null (SetSwitch)");
            return;
        }

        if (channel >= _instance.onSwitchChanged.Length || channel < 0)
        {
            Debug.LogWarning($"Invalid channel {channel} (SetSwitch)");
            return;
        }

        var isSwitchStateChanged = _instance._switches[(uint)channel] ^ value;
        _instance._switches[(uint)channel] = value;
        if (isSwitchStateChanged)
            _instance.onSwitchChanged[channel]?.Invoke(channel);
    }
    
    /// Reset all switches to false.
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
