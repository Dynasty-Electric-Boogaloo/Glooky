using System;
using UnityEngine;

/// Generic component that allows for creating actors that handle gravity, ground alignment, and directional movement.
/// The controller will try to make the rigidbody float at a certain distance from the ground when it is grounded,
/// in order to avoid friction with slopes, stairs and small bumps. Parameters can be adjusted to make the floating
/// reactions more or less springy.
public class PhysicsController : MonoBehaviour
{
    /// The maximum base speed the controller can move at.
    /// @note This is multiplied with the movementDirection, so the magnitude of the latter can be used to boost or reduce the current maxSpeed.
    [Header("Movement parameters")]
    [Tooltip("The maximum base speed the controller can move at.")]
    [SerializeField] private float maxSpeed;
    /// The acceleration the controller will try to apply according to the current velocity and targeted velocity.
    [Tooltip("The acceleration the controller will try to apply according to the current velocity and targeted velocity.")]
    [SerializeField] private float acceleration;
    /// The maximum acceleration the controller will be able to apply.
    [Tooltip("The maximum acceleration the controller will be able to apply.")]
    [SerializeField] private float maxAcceleration;
    
    [Header("Falling parameters")]
    [SerializeField] private float gravity;
    /// The desired height at which the controller tries to float.
    /// This is to avoid friction with slopes and stairs, alongside other effects.
    [Tooltip("The desired height at which the controller tries to float.\nThis is to avoid friction with slopes and stairs, alongside other effects.")]
    [SerializeField] private float hoverHeight;
    /// The force multiplier with which the controller will try to adjust its height to hover at the desired hoverHeight.
    [Tooltip("The force multiplier with which the controller will try to adjust its height to hover at the desired hoverHeight.")]
    [SerializeField] private float hoverForce;
    /// The force multiplier which will damp the hover forces and control how fast the controller will get to a stable float.
    [Tooltip("The force multiplier which will damp the hover forces and control how fast the controller will get to a stable float.")]
    [SerializeField] private float hoverDamper;
    /// The radius of the sphere that will check for ground.
    /// Make it only slightly smaller than the controller's collider, to prevent walls from being considered floor.
    [Tooltip("The radius of the sphere that will check for ground.\nMake it only slightly smaller than the controller's collider, to prevent walls from being considered floor.")]
    [SerializeField] private float groundCheckRadius;
    /// The maximum distance to check for ground when currently grounded.
    /// Make it long to stick to uneven ground and slopes, but not too long as to stick to the floor and never actually fall.
    [Tooltip("The maximum distance to check for ground when currently grounded.\nMake it long to stick to uneven ground and slopes, but not too long as to stick to the floor and never actually fall.")]
    [SerializeField] private float groundCheckLength;
    /// The maximum distance to check for ground when currently grounded.
    /// Make it relatively short to stick to the ground only when landing.
    [Tooltip("The maximum distance to check for ground when currently grounded.\nMake it relatively short to stick to the ground only when landing.")]
    [SerializeField] private float airborneCheckLength;
    /// Mask that drives which colliders are considered as "Ground".
    [Tooltip("Mask that drives which colliders are considered as \"Ground\".")]
    [SerializeField] private LayerMask groundMask;
    private Rigidbody _rigidbody;
    private Vector3 _movementDirection;
    private Vector3 _targetVelocity;
    private bool _grounded;
    private float _distanceFromHover;
    private Rigidbody _groundBody;
    

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        HandleMovement();
        GroundCheck();
    }

    /// Set horizontal target movement direction.
    /// <param name="direction">Movement direction, whose magnitude is a multiplier on maxSpeed. Y axis is ignored.</param>
    public void SetMovementDirection(Vector3 direction)
    {
        _movementDirection = direction;
        _movementDirection.y = 0;
    }

    public float GetDistanceFromHoverHeigh()
    {
        return _grounded ? _distanceFromHover : 0;
    }

    private void HandleMovement()
    {
        var maxVelocity = _movementDirection * maxSpeed;
        var relativeVel = _rigidbody.linearVelocity - (_groundBody ? _groundBody.linearVelocity : Vector3.zero);
        _targetVelocity = Vector3.MoveTowards(_targetVelocity, maxVelocity, acceleration * Time.fixedDeltaTime);
        var accelDiff = (_targetVelocity - relativeVel) / Time.fixedDeltaTime;
        accelDiff = Vector3.ClampMagnitude(accelDiff, maxAcceleration);
        _rigidbody.AddForce(Vector3.Scale(accelDiff, new Vector3(1, 0, 1)), ForceMode.Acceleration);
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

        _distanceFromHover = hit.distance - hoverHeight;
        var relVel = Vector3.Dot(ray.direction, _rigidbody.linearVelocity);

        var force = _distanceFromHover * hoverForce - relVel * hoverDamper;
        _rigidbody.AddForce(ray.direction * force, ForceMode.Acceleration);

        _groundBody = hit.rigidbody;
        if (_groundBody)
            hit.rigidbody.AddForceAtPosition(ray.direction * _rigidbody.mass, hit.point, ForceMode.Acceleration);
    }
}