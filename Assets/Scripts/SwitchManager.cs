using System;
using UnityEngine;
using UnityEngine.Events;
using TriInspector;

/// Singleton component that holds the switch array and handles its calls.
[HideMonoScript]
public class SwitchManager : MonoBehaviour
{
    private static SwitchManager _instance;
    
    private float[] _switches = new float[128];
    
    [SerializeField] private UnityEvent<int>[] onSwitchChanged = new UnityEvent<int>[128];
    
    private void Awake()
    {
        if (_instance)
        {
            Destroy(this);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(_instance);
    }
    
    /// Set a callback to listen for changes to the switch at the specified channel identifier.
    /// <param name="call">Callback function called when this switch is changed.</param>
    /// <param name="channel">Switch channel to listen to.</param>
    public static void AddListenerOnChannel(UnityAction<int> call, int channel)
    {
        if (!_instance)
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
    
    /// Stop a callback from listening to changes to the switch at the specified channel identifier.
    /// <param name="call">Callback function called when this switch is changed.</param>
    /// <param name="channel">Switch channel to stop listening to.</param>
    public static void RemoveListenerOnChannel(UnityAction<int> call, int channel)
    {
        if (!_instance)
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
    /// <param name="channel">The channel from which to get the switch's state.</param>
    /// <returns>The channel's state.</returns>
    public static bool GetSwitch(int channel)
    {
        if (!_instance)
        {
            Debug.LogWarning($"SwitchManager instance is null (GetSwitch)");
            return false;
        }

        if (channel >= _instance.onSwitchChanged.Length || channel < 0)
        {
            Debug.LogWarning($"Invalid channel {channel} (GetSwitch)");
            return false;
        }
        
        return _instance._switches[(uint)channel] > 0.5f;
    }
    
    /// Get switch's floating value using specified channel identifier.
    /// <param name="channel">The channel from which to get the switch's state.</param>
    /// <returns>The channel's state.</returns>
    public static float GetSwitchFloat(int channel)
    {
        if (!_instance)
        {
            Debug.LogWarning($"SwitchManager instance is null (GetSwitch)");
            return 0;
        }

        if (channel >= _instance.onSwitchChanged.Length || channel < 0)
        {
            Debug.LogWarning($"Invalid channel {channel} (GetSwitch)");
            return 0;
        }
        
        return _instance._switches[(uint)channel];
    }
    
    /// Set switch's state using specified channel identifier. Invokes onSwitchChanged if the switch state changes.
    /// <param name="channel">Switch to be set.</param>
    /// <param name="value">New Switch value.</param>
    public static void SetSwitch(int channel, bool value)
    {
        if (!_instance)
        {
            Debug.LogWarning($"SwitchManager instance is null (SetSwitch)");
            return;
        }

        if (channel >= _instance.onSwitchChanged.Length || channel < 0)
        {
            Debug.LogWarning($"Invalid channel {channel} (SetSwitch)");
            return;
        }
        
        var floatValue = value ? 1 : 0;
        var isSwitchStateChanged = (_instance._switches[(uint)channel] > 0.5f) ^ value;
        _instance._switches[(uint)channel] = floatValue;
        if (isSwitchStateChanged)
            _instance.onSwitchChanged[channel]?.Invoke(channel);
    }
    
    /// Set switch's floating value using specified channel identifier.
    /// Invokes onSwitchChanged if the switch floating value crosses the halfway threshold.
    /// <param name="channel">Switch to be set.</param>
    /// <param name="value">New Switch value.</param>
    public static void SetSwitchFloat(int channel, float value)
    {
        if (!_instance)
        {
            Debug.LogWarning($"SwitchManager instance is null (SetSwitch)");
            return;
        }

        if (channel >= _instance.onSwitchChanged.Length || channel < 0)
        {
            Debug.LogWarning($"Invalid channel {channel} (SetSwitch)");
            return;
        }

        value = Mathf.Clamp01(value);
        var isSwitchStateChanged = (_instance._switches[(uint)channel] > 0.5f) ^ (value > 0.5f);
        _instance._switches[(uint)channel] = value;
        if (isSwitchStateChanged)
            _instance.onSwitchChanged[channel]?.Invoke(channel);
    }
    
    /// Reset all switches.
    public static void ResetSwitches()
    {
        if (!_instance)
        {
            Debug.LogWarning($"SwitchManager instance is null (ResetSwitches)");
            return;
        }
        
        Array.Fill(_instance._switches, 0);
    }
}
