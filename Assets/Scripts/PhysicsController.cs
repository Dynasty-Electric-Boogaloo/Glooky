using System;
using UnityEngine;

public class PhysicsController : MonoBehaviour
{
    [SerializeField] private float maxSpeed;
    [SerializeField] private float acceleration;
    [SerializeField] private float maxAcceleration;
    
    [Header("Falling parameters")]
    [SerializeField] private float gravity;
    [SerializeField] private float hoverHeight;
    [SerializeField] private float hoverForce;
    [SerializeField] private float hoverDamper;
    [SerializeField] private float groundCheckRadius;
    [SerializeField] private float groundCheckLength;
    [SerializeField] private float airborneCheckLength;
    [SerializeField] private LayerMask groundMask;
    private Rigidbody _rigidbody;
    private Vector3 _movementDirection;
    private Vector3 _targetVelocity;
    private bool _grounded;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        HandleMovement();
        GroundCheck();
    }

    public void SetMovementDirection(Vector3 direction)
    {
        _movementDirection = direction;
        _movementDirection.y = 0;
    }

    private void HandleMovement()
    {
        var maxVelocity = _movementDirection * maxSpeed;
        _targetVelocity = Vector3.MoveTowards(_targetVelocity, maxVelocity, acceleration * Time.fixedDeltaTime);
        var accelDiff = (_targetVelocity - _rigidbody.linearVelocity) / Time.fixedDeltaTime;
        accelDiff = Vector3.ClampMagnitude(accelDiff, maxAcceleration);
        _rigidbody.AddForce(Vector3.Scale(accelDiff, new Vector3(1, 0, 1)), ForceMode.Force);
    }

    private void GroundCheck()
    {
        var ray = new Ray(_rigidbody.position + Vector3.up * groundCheckRadius, Vector3.down);

        var checkLength = _grounded ? groundCheckLength : airborneCheckLength;
        _grounded = Physics.SphereCast(ray, groundCheckRadius, out var hit, checkLength, groundMask);
        
        if (!_grounded)
        {
            _rigidbody.AddForce(Vector3.down * gravity, ForceMode.Acceleration);
            return;
        }

        var heightDiff = hit.distance - hoverHeight - groundCheckRadius;
        var relVel = Vector3.Dot(ray.direction, _rigidbody.linearVelocity);

        var force = heightDiff * hoverForce - relVel * hoverDamper;
        _rigidbody.AddForce(ray.direction * force, ForceMode.Acceleration);
    }
}