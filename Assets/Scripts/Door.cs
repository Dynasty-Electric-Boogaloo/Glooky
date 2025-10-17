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
        _moveSpeed = 10f;
    }
    
    private void OnEnable()
    {
        SwitchManager.AddListenerOnChannel(OnSwitchChanged, channelListenedTo);
    }

    private void OnDisable()
    {
        SwitchManager.RemoveListenerOnChannel(OnSwitchChanged, channelListenedTo);
    }
    
    private void FixedUpdate()
    {
        if ((transform.position - _targetPosition).sqrMagnitude > Mathf.Epsilon)
            UpdatePosition();
    }

    private void UpdatePosition()
    {
        transform.position = Vector3.Lerp(transform.position, _targetPosition, _moveSpeed * Time.fixedDeltaTime);
    }
    
    /// Execute behaviour.
    /// <param name="channel">The channel ID.</param>
    private void OnSwitchChanged(int channel)
    {
        if (channel != channelListenedTo)
            return;
        
        ExecuteBehaviour();
    }

    private void ExecuteBehaviour()
    {
        if (SwitchManager.GetSwitch(channelListenedTo))
        {
            _targetPosition = _initialPosition - new Vector3(0, 3f, 0);
            return;
        }
        
        _targetPosition = _initialPosition;
    }
}