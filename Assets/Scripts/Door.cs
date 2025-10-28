using System;
using UnityEngine;

/// Door behaviour. Can receive channel calls to open.
public class Door : MonoBehaviour
{
    [SerializeField] private int channelListenedTo;
    
    // temp variable for showcase
    private Vector3 _initialPosition;
    private Vector3 _targetPosition;
    private float _moveSpeed;

    private void Awake()
    {
        _initialPosition = transform.position;
        _targetPosition = transform.position;
        _moveSpeed = 5f;
    }
    
    private void FixedUpdate()
    {
        _targetPosition = Vector3.Lerp(_initialPosition, _initialPosition - new Vector3(0, 2f, 0),
            SwitchManager.GetSwitchFloat(channelListenedTo));
        
        UpdatePosition();
    }

    private void UpdatePosition()
    {
        transform.position = Vector3.MoveTowards(transform.position, _targetPosition, _moveSpeed * Time.fixedDeltaTime);
    }
}