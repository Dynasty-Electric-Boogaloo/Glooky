using System;
using System.Collections.Generic;
using UnityEngine;

/// Pressure plate behaviour.
public class PressurePlateInteractable : Interactable
{
    [SerializeField] private int switchChannel;
    
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
        var shouldBePressed = _physicsController.GetDistanceFromHoverHeight() > 0.5f;
        Interact(shouldBePressed);
    }
    
    public override void Interact(bool isInteracting)
    {
        if (!(_activated ^ isInteracting))
            return;
        
        _activated = isInteracting;
        SwitchManager.SetSwitch(switchChannel, _activated);
    }
}