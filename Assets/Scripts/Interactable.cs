using UnityEngine;

public class Interactable : MonoBehaviour, IClickable
{
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

    public ClickableType GetClickableType()
    {
        return ClickableType.Interactable;
    }
}