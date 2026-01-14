using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Collection/Item Database")]
public sealed class ItemDatabase : ScriptableObject
{
    [SerializeField] private List<ItemDefinition> items = new();

    private Dictionary<string, ItemDefinition> _byId;

    public IReadOnlyList<ItemDefinition> Items => items;

    private void OnEnable()
    {
        BuildIndex();
    }


    public void BuildIndex()
    {
        _byId = new Dictionary<string, ItemDefinition>(items.Count);

        for (int i = 0; i < items.Count; i++)
        {
            var def = items[i];
            if (def == null) continue;

            if (string.IsNullOrEmpty(def.Id))
            {
                Debug.LogWarning($"ItemDatabase: Item at index {i} has empty Id.", this);
                continue;
            }

            if (_byId.ContainsKey(def.Id))
            {
                Debug.LogError($"ItemDatabase: Duplicate item id '{def.Id}'.", this);
                continue;
            }

            _byId.Add(def.Id, def);
        }
    }

    public bool TryGet(string id, out ItemDefinition def)
    {
        def = null;

        if (_byId == null)
            BuildIndex();

        if (string.IsNullOrEmpty(id))
            return false;

        return _byId.TryGetValue(id, out def);
    }

#if UNITY_EDITOR
    public void RebuildFromFolder(string folderPath)
    {
        items.Clear();

        var guids = UnityEditor.AssetDatabase.FindAssets("t:ItemDefinition", new[] { folderPath });
        foreach (var guid in guids)
        {
            var path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
            var def = UnityEditor.AssetDatabase.LoadAssetAtPath<ItemDefinition>(path);
            if (def != null) items.Add(def);
        }

        items.Sort((a, b) => string.CompareOrdinal(a.Id, b.Id));

        UnityEditor.EditorUtility.SetDirty(this);
        UnityEditor.AssetDatabase.SaveAssets();

        BuildIndex();
        Debug.Log($"ItemDatabase rebuilt: {items.Count} items.", this);
    }
#endif
}

