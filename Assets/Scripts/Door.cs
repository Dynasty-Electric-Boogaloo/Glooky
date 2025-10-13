using System;
using UnityEngine;

/// Base class for Interactable objects.
// TODO Implement Host interaction.
public class Door : MonoBehaviour
{
    [SerializeField] private int channelListenedTo;
    
    // temp variable for showcase
    private Vector3 _targetPosition;
    private float _slowDownSpeed;

    private void Awake()
    {
        _targetPosition = transform.position;
        _slowDownSpeed = 10f;
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
        transform.position = Vector3.Lerp(transform.position, _targetPosition, _slowDownSpeed * Time.fixedDeltaTime);
    }
    
    /// Execute behaviour.
    /// <param name="channel">The channel ID.</param>
    private void OnSwitchChanged(int channel)
    {
        if (channel != channelListenedTo)
            return;
        
        OpenOrCloseDoor();
    }

    private void OpenOrCloseDoor()
    {
        if (SwitchManager.GetSwitch(channelListenedTo))
        {
            _targetPosition = transform.position - new Vector3(0, 3f, 0);
            return;
        }
        
        _targetPosition = transform.position + new Vector3(0, 3f, 0);
    }
}