/// Identifies the type of interaction the cursor can have with an #IClickable object.
public enum ClickableType
{
    /// Host object, should be possessable by CursorController.
    Host,
    /// Interactable object, should be able to be interacted on by a Host.
    Interactable,
}