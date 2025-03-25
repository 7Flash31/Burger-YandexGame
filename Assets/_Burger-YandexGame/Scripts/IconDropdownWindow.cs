using UnityEngine;
using UnityEditor;
using System;

#if UNITY_EDITOR
public class IconDropdownWindow : EditorWindow
{
    private GUIContent[] items;
    private Action<int> onSelect;

    public static void ShowDropdown(EditorWindow parent, GUIContent[] items, Action<int> onSelect)
    {
        var window = CreateInstance<IconDropdownWindow>();
        window.titleContent = new GUIContent("Icon Dropdown");
        window.items = items;
        window.onSelect = onSelect;

        float height = items.Length * 22f + 10f;
        float width = 200f;

        var parentRect = parent.position;

        float x = parentRect.x + parentRect.width + 10f;
        float y = parentRect.y;

        window.position = new Rect(x, y, width, height);

        window.Show();
    }

    private void OnGUI()
    {
        if(items == null) return;

        for(int i = 0; i < items.Length; i++)
        {
            if(GUILayout.Button(items[i], GUILayout.Height(20)))
            {
                onSelect?.Invoke(i);
                Close();
            }
        }
    }
}
#endif
