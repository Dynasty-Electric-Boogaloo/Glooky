using System;
using UnityEngine;

/// TEST Class
/// Door behaviour. Can receive channel calls to open.
//TEST Class do not use.
public class Door : MonoBehaviour
{
    /// Channel being listened to.
    [Tooltip("Channel being listened to.")]
    [SceneLabel("", 1f, 0f, 0f, 32)]
    [SerializeField] private int inputChannel;
    
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
            SwitchManager.GetSwitchFloat(inputChannel));
        
        UpdatePosition();
    }

    private void UpdatePosition()
    {
        transform.position = Vector3.MoveTowards(transform.position, _targetPosition, _moveSpeed * Time.fixedDeltaTime);
    }
}