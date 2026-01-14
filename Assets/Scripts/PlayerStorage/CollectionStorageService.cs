using System.Collections.Generic;
using System.IO;
using UnityEngine;

public sealed class CollectionStorageService
{
    public struct State { public int amount; public bool seen; }

    private readonly ItemDatabase _db;
    private readonly Dictionary<string, State> _state;
    private readonly string _path;
    private bool _dirty;

    public CollectionStorageService(ItemDatabase db)
    {
        _db = db;
        _db.BuildIndex();
        _state = new Dictionary<string, State>(_db.Items.Count);
        _path = Path.Combine(Application.persistentDataPath, "collection.json");
    }

    public void Load()
    {
        _state.Clear();

        foreach (var def in _db.Items)
            _state[def.Id] = new State { amount = 0, seen = false };

        if (!File.Exists(_path)) return;

        try
        {
            var json = File.ReadAllText(_path);
            var save = JsonUtility.FromJson<CollectionSave>(json);
            if (save?.entries == null) return;

            foreach (var e in save.entries)
            {
                if (string.IsNullOrEmpty(e.id)) continue;
                if (_state.ContainsKey(e.id))
                    _state[e.id] = new State { amount = e.amount, seen = e.seen };
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning($"collection load failed: {ex}");
        }
    }

    public void Add(string id, int delta = 1)
    {
        if (!_state.TryGetValue(id, out var s)) return;
        s.amount += delta;
        _state[id] = s;
        _dirty = true;
    }

    public void MarkSeen(string id)
    {
        if (!_state.TryGetValue(id, out var s)) return;
        if (s.seen) return;
        s.seen = true;
        _state[id] = s;
        _dirty = true;
    }

    public bool TryGetState(string id, out State state) => _state.TryGetValue(id, out state);

    public void SaveIfDirty()
    {
        if (!_dirty) return;
        SaveNow();
        _dirty = false;
    }

    private void SaveNow()
    {
        try
        {
            var save = new CollectionSave();

            foreach (var kv in _state)
            {
                if (kv.Value.amount == 0 && !kv.Value.seen) continue;
                save.entries.Add(new Entry { id = kv.Key, amount = kv.Value.amount, seen = kv.Value.seen });
            }

            var json = JsonUtility.ToJson(save, prettyPrint: false);

            var tmp = _path + ".tmp";
            File.WriteAllText(tmp, json);
            if (File.Exists(_path)) File.Delete(_path);
            File.Move(tmp, _path);
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning($"Collection save failed: {ex}");
        }
    }
}
