using System;
using UnityEngine;

public class SplineFollower : MonoBehaviour
{
    [SerializeField] private SplineTrack track;
    [SerializeField] private float speed;
    [SerializeField] private float speedDamper;
    [SerializeField] private float acceleration;
    [SerializeField] private float maxAcceleration;
    [SerializeField] private float hoverForce;
    [SerializeField] private float hoverDamper;
    [SerializeField] private float trackProgressDistanceThreshold;
    [SerializeField] private float trackProgressSpeed;
    
    private Rigidbody _rigidbody;
    private Vector3 _targetVelocity;
    private float _factor;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        var splinePos = track.GetPosition(_factor);
        HandleHoverForce(splinePos);
        HandleMovementForce(splinePos);
        HandleFactorProgress(splinePos);
    }

    private void HandleHoverForce(Vector3 targetPosition)
    {
        var diff = targetPosition.y - _rigidbody.position.y;
        
        var relVel = Vector3.Dot(Vector3.up, _rigidbody.linearVelocity);
        var force = diff * hoverForce - relVel * hoverDamper;
        _rigidbody.AddForce(Vector3.up * force, ForceMode.Acceleration);
    }

    private void HandleMovementForce(Vector3 targetPosition)
    {
        var diff = targetPosition - _rigidbody.position;
        diff.y = 0;
        var maxSpeed = diff.normalized * speed;
        
        var relVel = _rigidbody.linearVelocity;
        relVel.y = 0;
        
        _targetVelocity = Vector3.MoveTowards(_targetVelocity, maxSpeed, acceleration * Time.fixedDeltaTime);
        var accelDiff = (_targetVelocity - relVel) / Time.fixedDeltaTime;
        accelDiff = Vector3.ClampMagnitude(accelDiff, maxAcceleration);
        var force = accelDiff - relVel * speedDamper;
        
        _rigidbody.AddForce(force, ForceMode.Acceleration);
    }
    
    private void HandleFactorProgress(Vector3 splinePos)
    {
        if (Vector3.Distance(splinePos, _rigidbody.position) > trackProgressDistanceThreshold)
            return;
        
        _factor += Time.fixedDeltaTime / track.GetLength() * trackProgressSpeed;
        _factor %= 1f;
    }
}