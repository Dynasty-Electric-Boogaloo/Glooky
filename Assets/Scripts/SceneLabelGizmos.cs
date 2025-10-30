using UnityEditor;
using UnityEngine;

[ExecuteAlways]
public class SceneLabelGizmos : MonoBehaviour
{
    private void OnDrawGizmos()
    {
        var sceneActive = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);

        foreach (var mono in sceneActive)
        {
            var type = mono.GetType();
            var fields = type.GetFields(System.Reflection.BindingFlags.Public |
                                        System.Reflection.BindingFlags.NonPublic |
                                        System.Reflection.BindingFlags.Instance);

            foreach (var field in fields)
            {
                var attr = (SceneLabelAttribute)System.Attribute.GetCustomAttribute(field,
                    typeof(SceneLabelAttribute));
                if (attr == null) continue;

                var value = field.GetValue(mono);
                var text = attr.Text;
                if (string.IsNullOrEmpty(text))
                {
                    text = value.ToString();
                }
                else
                {
                    if (value != null)
                        text += ": " + value;
                }

                var pos = mono.transform.position + Vector3.up;

                var style = new GUIStyle(EditorStyles.boldLabel)
                {
                    fontSize = attr.FontSize,
                    normal = { textColor = attr.Color },
                    alignment = TextAnchor.MiddleCenter
                };

                Handles.Label(pos, text, style);
            }
        }
    }
}