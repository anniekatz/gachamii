#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ItemDatabase))]
public sealed class ItemDatabaseEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        var db = (ItemDatabase)target;

        GUILayout.Space(10);
        if (GUILayout.Button("Rebuild From Folder: Assets/Collection/Definitions"))
        {
            db.RebuildFromFolder("Assets/Collection/Definitions");
        }
    }
}
#endif
