using System;
using System.Collections.Generic;
using UnityEngine;

/// Base class for Interactable objects.
// TODO Implement Host interaction.
public class PressurePlateInteractable : Interactable
{
    [SerializeField] private int switchChannel;
    
    private Collider[] _hitColliders;
    private bool _activated = false;
    private Vector3 _initialPosition;
    private Vector3 _targetPosition;
    private float _moveSpeed;

    private void Awake()
    {
        _hitColliders = new Collider[5];
        _initialPosition = transform.position;
        _targetPosition = transform.position;
        _moveSpeed = 10f;
    }

    private void FixedUpdate()
    {
        CheckPressure();
        // Interact(Physics.CheckSphere(transform.position + Vector3.up * 0.5f, 0.05f, LayerMask.GetMask("Ground")));
        
        if (transform.position != _targetPosition)
            UpdatePosition();
    }

    private void UpdatePosition()
    {
        transform.position = Vector3.Lerp(transform.position, _targetPosition, _moveSpeed * Time.fixedDeltaTime);
    }

    private void CheckPressure()
    {
        var colliderCount = Physics.OverlapBoxNonAlloc(
            transform.position,
            new Vector3(0.5f, 5f, 0.5f), _hitColliders
        );
        
        var rigidbodies = new List<Rigidbody>();
        for (var i = 0; i < colliderCount; i++)
        {
            if (_hitColliders[i].attachedRigidbody && !rigidbodies.Contains(_hitColliders[i].attachedRigidbody))
                rigidbodies.Add(_hitColliders[i].attachedRigidbody);
        }
        
        var totalDownwardForce = 0f;
        foreach (var rb in rigidbodies)
        {
            rb.AddForce(Physics.gravity, ForceMode.Acceleration);
            var accumulatedForce = rb.GetAccumulatedForce();
            print(accumulatedForce);

            var downward = Vector3.Dot(accumulatedForce, Physics.gravity.normalized);
            if (downward > 0f)
                totalDownwardForce += downward;
        }
        
        var shouldBePressed = totalDownwardForce > 50f;
        Interact(shouldBePressed);
    }
    
    public override void Interact(bool isInteracting)
    {
        if (!(_activated ^ isInteracting))
            return;
        
        _activated = isInteracting;
        SwitchManager.SetSwitch(switchChannel, _activated);

        ExecuteBehaviour();
    }

    private void ExecuteBehaviour()
    {
        if (_activated)
        {
            _targetPosition = _initialPosition - new Vector3(0, 0.3f, 0);
            return;
        }
        
        _targetPosition = _initialPosition;
    }
}