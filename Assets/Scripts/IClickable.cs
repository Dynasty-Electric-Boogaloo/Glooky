using DefaultNamespace;

public interface IClickable
{
    public bool Click(CursorController controller);
    public ClickableType GetClickableType();
}