using System;
using UnityEngine;
using TriInspector;

/// Door behaviour. Position when open and speed can be configured.
/// Can receive channel calls to open.
[HideMonoScript]
[DeclareHorizontalGroup("door/configureOpenDoorButtons")]
public class Door : MonoBehaviour
{
    /// Channel being listened to.
    [Tooltip("Channel being listened to.")]
    [SerializeField] private int inputChannel;
    
    /// Door opening and closing speed.
    [Tooltip("Door opening and closing speed.")]
    [SerializeField] private float speed = 5f;
    
    /// Position of the door when open.
    [Tooltip("Position of the door when open.")]
    [PropertyOrder(4)]
#if UNITY_EDITOR
    [InfoBox("Move the Door to the desired position and confirm.", visibleIf: nameof(_configuringDoorOpenPosition))]
    [ValidateInput(nameof(IsDoorOpenPositionConfigured))]
#endif
    [ReadOnly]
    [SerializeField] private Vector3 openPosition;
    
    [SerializeField, HideInInspector] private bool isDoorOpenPositionConfigured = false;
    private Vector3 _closedPosition;
    private Vector3 _targetPosition;

    private void Awake()
    {
        if (!isDoorOpenPositionConfigured)
            openPosition = transform.position + Vector3.down * 2f;
        _closedPosition = transform.position;
    }
    
    private void FixedUpdate()
    {
        _targetPosition = Vector3.Lerp(_closedPosition, openPosition,
            SwitchManager.GetSwitchFloat(inputChannel));
        
        UpdatePosition();
    }

    private void UpdatePosition()
    {
        transform.position = Vector3.MoveTowards(transform.position, _targetPosition, speed * Time.fixedDeltaTime);
    }

#if UNITY_EDITOR
    private bool _configuringDoorOpenPosition = false;
    
    /// EDITOR TOOL
    /// Internal function for easy open position configuration.
    [PropertyOrder(2)]
    [HideInPlayMode]
    [Button(ButtonSizes.Medium, "$" + nameof(ConfigureDoorOpenPositionLabelName))]
    [Group("door/configureOpenDoorButtons")]
    private void ConfigureDoorOpenPosition()
    {
        // Start configure
        if (!_configuringDoorOpenPosition)
        {
            if (!isDoorOpenPositionConfigured)
            {
                openPosition = transform.position;
                isDoorOpenPositionConfigured = true;
            }
            
            _closedPosition = transform.position;
            transform.position = openPosition;
            _configuringDoorOpenPosition = true;
            return;
        }
        
        // Confirm position
        openPosition = transform.position;
        transform.position = _closedPosition;
        _configuringDoorOpenPosition = false;
    }

    private string ConfigureDoorOpenPositionLabelName()
    {
        return _configuringDoorOpenPosition ? "Confirm" : "Configure Door Open Position";
    }
    
    /// EDITOR TOOL
    /// Internal function for easy open position configuration.
    /// Cancels the current modification.
    [PropertyOrder(3)]
    [HideInPlayMode]
    [GUIColor(1.0f, 0.6f, 0.6f)]
    [Button(ButtonSizes.Medium, "Cancel")]
    [Group("door/configureOpenDoorButtons")]
    [HideIf(nameof(_configuringDoorOpenPosition), false)]
    private void CancelConfigureDoorOpenPosition()
    {
        // Cancel configure
        transform.position = _closedPosition;
        _configuringDoorOpenPosition = false;
    }
    
    /// EDITOR TOOL
    /// Display warning to user if door open position has not been defined.
    private TriValidationResult IsDoorOpenPositionConfigured()
    {
        if (isDoorOpenPositionConfigured)
            return TriValidationResult.Valid;
        return TriValidationResult.Warning("Door Open Position is not configured. Open position will be set to position.y - 2 in play mode by default.");
    }
#endif
}