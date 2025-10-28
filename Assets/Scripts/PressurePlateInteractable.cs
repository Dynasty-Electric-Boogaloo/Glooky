using System;
using System.Collections.Generic;
using UnityEngine;

/// Pressure plate behaviour.
public class PressurePlateInteractable : Interactable
{
    [SerializeField] private int outputChannel;
    
    private bool _activated = false;
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

    private void CheckPressure()
    {
        SwitchManager.SetSwitchFloat(outputChannel, _physicsController.GetDistanceFromHoverHeight());
    }
}