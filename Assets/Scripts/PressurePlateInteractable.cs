using UnityEngine;

/// Pressure plate behaviour.
public class PressurePlateInteractable : Interactable
{
    /// Channel which is being sent a signal to.
    [Tooltip("Channel which is being sent a signal to.")]
    [SceneLabel("", 0f, 1f, 0f, 32)]
    [SerializeField] private int outputChannel;
    /// How sensitive the pressure plate effect is.
    /// Value can't be 0 or less.
    /// The smaller the value, the more sensitive it is.
    [Tooltip("How sensitive the pressure plate effect is. Value can't be 0 or less.")]
    [Min(1e-07f)]
    [SerializeField] private float sensitivity = 1f;
    
    private PhysicsController _physicsController;

    private void Awake()
    {
        _physicsController = GetComponentInParent<PhysicsController>();

        if (!_physicsController)
        {
            Debug.LogError($"{name}: No PhysicsController found");
            enabled = false;
        }
    }

    private void FixedUpdate()
    {
        CheckPressure();
    }
    
    /// Set switch float value to output channel.
    /// The float value depends on how hard the pressure plate is pressed.
    private void CheckPressure()
    {
        SwitchManager.SetSwitchFloat(outputChannel, _physicsController.GetDistanceFromHoverHeight() / sensitivity);
    }
}