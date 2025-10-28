using System;
using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    [SerializeField] private float movementSpeed;
    private PhysicsController _physicsController;

    private void Awake()
    {
        _physicsController = GetComponent<PhysicsController>();
    }

    private void FixedUpdate()
    {
        _physicsController.SetMovementDirection(Vector3.forward * Mathf.Sign(Mathf.Sin(Time.time)));
    }
}