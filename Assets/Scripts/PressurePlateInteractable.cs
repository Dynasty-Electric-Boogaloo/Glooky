using System;
using UnityEngine;

/// Base class for Interactable objects.
// TODO Implement Host interaction.
public class PressurePlateInteractable : Interactable
{
    [SerializeField] private int switchChannel;
    
    private bool _activated = false;
    private Vector3 _targetPosition;
    private float _slowDownSpeed;

    private void Awake()
    {
        _targetPosition = transform.position;
        _slowDownSpeed = 10f;
    }

    private void FixedUpdate()
    {
        if ((transform.position - _targetPosition).sqrMagnitude > Mathf.Epsilon)
            UpdatePosition();
    }

    private void UpdatePosition()
    {
        transform.position = Vector3.Lerp(transform.position, _targetPosition, _slowDownSpeed * Time.fixedDeltaTime);
    }
    
    public override void Interact(bool isInteracting)
    {
        if (!(_activated ^ isInteracting))
            return;
        
        _activated = isInteracting;

        if (_activated)
        {
            SwitchManager.SetSwitch(switchChannel, true);
            _targetPosition = transform.position - new Vector3(0, 0.3f, 0);
            return;
        }
        
        SwitchManager.SetSwitch(switchChannel, false);
        _targetPosition = transform.position + new Vector3(0, 0.3f, 0);
    }
}