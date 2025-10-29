using System;
using System.Collections.Generic;
using UnityEngine;

/// Pressure plate behaviour.
public class PressurePlateInteractable : Interactable
{
    /// Channel which is being sent a signal to.
    [SerializeField] private int outputChannel;
    
    private PhysicsController _physicsController;

    private void Awake()
    {
        _physicsController = GetComponentInParent<PhysicsController>();

        if (!_physicsController)
        {
            Debug.LogError($"{this.name}: No PhysicsController found");
            this.enabled = false;
        }
    }

    private void FixedUpdate()
    {
        CheckPressure();
    }
    
    /// Set switch float value to output channel.
    /// The float value depends on how hard the pressure plate is pressed.
    private void CheckPressure()
    {
        SwitchManager.SetSwitchFloat(outputChannel, _physicsController.GetDistanceFromHoverHeight());
    }
}