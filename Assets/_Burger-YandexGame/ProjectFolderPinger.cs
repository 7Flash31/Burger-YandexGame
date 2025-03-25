using UnityEngine;
using UnityEditor;

public static class ProjectFolderPinger
{
    [MenuItem("Tools/Ping Folder")]
    public static void PingFolder()
    {
        // Укажите путь к папке относительно Assets
        string folderPath = "Assets/MyFolder";
        Object folder = AssetDatabase.LoadAssetAtPath<Object>(folderPath);
        if(folder != null)
        {
            EditorGUIUtility.PingObject(folder);
        }
        else
        {
            Debug.LogWarning("Папка не найдена: " + folderPath);
        }
    }
}
