/// Interface from which every CursorController clickable element must inherit.
/// @note Make sure that the GameObject containing components that inherit IClickable have a collider and the proper Layer.
/// <seealso cref="Host"/>
public interface IClickable
{
    /// Function called when CursorController detects and clicks on a GameObject containing an IClickable component.
    public bool Click(CursorController controller);
    
    /// Get the IClickable component's type.
    public ClickableType GetClickableType();
}