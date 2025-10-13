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
    
    /// <summary>
    /// Set MonoBehaviour to start listening to calls from a specified channel.
    /// </summary>
    /// <param name="call">Function called when this channel calls.</param>
    /// <param name="channel">Channel to start listening to.</param>
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
    
    /// <summary>
    /// Set MonoBehaviour to stop listening to calls from a specified channel.
    /// </summary>
    /// <param name="call">Function called when this channel calls.</param>
    /// <param name="channel">Channel to stop listening to.</param>
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
    
    /// <summary>
    /// Get a specified channel's state.
    /// </summary>
    /// <param name="channel">The channel state to get.</param>
    /// <returns>The channel's state.</returns>
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
    
    /// <summary>
    /// Set a specified channel's state. Also invoke a call from the same channel.
    /// </summary>
    /// <param name="channel">Switch to set and invoke a call from.</param>
    /// <param name="value">New Switch value.</param>
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
    
    /// <summary>
    /// Reset all switches to false.
    /// </summary>
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
