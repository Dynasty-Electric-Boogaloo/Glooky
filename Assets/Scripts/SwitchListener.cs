using System;
using TriInspector;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering;
using UnityEngine.Serialization;

[HideMonoScript]
public class SwitchListener : MonoBehaviour
{
    public UnityEvent<bool> onSwitchCallReceived;
    
    [OnValueChanged(nameof(UpdateSwitchCondition))]
    [ListDrawerSettings(AlwaysExpanded = true)]
    [SerializeField] private uint[] switchCondition;
    
    // prefix h to describe a saved, internal value
    [HideInInspector]
    [SerializeField] private BitArray128 hSwitchCondition;

    private void OnEnable()
    {
        SwitchManager.Instance.onSwitchCalled.AddListener(OnSwitchCalled);
        OnSwitchCalled(SwitchManager.Instance.globalSwitch);
    }

    private void OnDisable()
    {
        SwitchManager.Instance.onSwitchCalled.RemoveListener(OnSwitchCalled);
    }

    private void OnSwitchCalled(BitArray128 condition)
    {
        bool conditionMatched = (condition & hSwitchCondition).Equals(hSwitchCondition);
        onSwitchCallReceived?.Invoke(conditionMatched);
    }

    private void UpdateSwitchCondition()
    {
        hSwitchCondition = new BitArray128();
        foreach (var i in switchCondition)
            hSwitchCondition[i] = true;
    }
}
