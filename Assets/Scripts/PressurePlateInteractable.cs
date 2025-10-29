using System;
using System.Collections.Generic;
using UnityEngine;

/// Pressure plate behaviour.
public class PressurePlateInteractable : Interactable
{
    /// Channel which is being sent a signal to.
    [Tooltip("Channel which is being sent a signal to.")]
    [SerializeField] private int outputChannel;
    /// How sensitive the pressure plate effect is.
    /// A value of 0 means the pressure plate won't trigger anything.
    /// A value of 1 means the pressure plate will send its full effect when fully pressed down.
    /// A value of 2 means the pressure plate can be half pressed and will still send its full effect.
    [Tooltip("How sensitive the pressure plate effect is.")]
    [SerializeField] private float sensitivity = 1f;
    
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
        SwitchManager.SetSwitchFloat(outputChannel, sensitivity * _physicsController.GetDistanceFromHoverHeight());
    }
}