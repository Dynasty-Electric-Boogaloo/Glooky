using UnityEngine;

/// Base class for Interactable objects.
// TODO Implement Host interaction.
public class Interactable : MonoBehaviour, IClickable
{
    /// Get the point which the Host will try to get to in order to interact.
    /// <returns>Interaction target point.</returns>
    public virtual Vector3 GetTargetPoint()
    {
        return transform.position;
    }
    
    public bool Click(CursorController controller)
    {
        var host = controller.GetHost();
        if (!host)
            return false;
        
        host.BeginInteraction(this);
        return true;
    }

    public virtual void Interact(bool isInteracting) {}

    public ClickableType GetClickableType()
    {
        return ClickableType.Interactable;
    }
}