using UnityEngine;

[System.AttributeUsage(System.AttributeTargets.Field)]
public class SceneLabelAttribute : PropertyAttribute
{
    public string Text;
    public Color Color;
    public int FontSize;

    public SceneLabelAttribute(string text = "", float r = 1f, float g = 1f, float b = 1f, int fontSize = 20)
    {
        Text = text;
        Color = new Color(r, g, b);
        FontSize = fontSize;
    }
}