using System;
using UnityEngine;

//TEST Class do not use.
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