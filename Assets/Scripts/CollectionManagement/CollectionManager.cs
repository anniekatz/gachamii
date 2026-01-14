using UnityEngine;
using System.Collections.Generic;

public sealed class CollectionManager : MonoBehaviour
{
    [SerializeField] private ItemDatabase database;
    public ItemDatabase Database => database;

    public static CollectionManager Instance { get; private set; }

    private CollectionStorageService _service;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        database.BuildIndex();
        _service = new CollectionStorageService(database);
        _service.Load();
    }

    public void AddItem(string id, int amount = 1, bool markSeen = true)
    {
        _service.Add(id, amount);
        if (markSeen) _service.MarkSeen(id);

        _service.SaveIfDirty();
    }

    public bool TryGetState(string id, out CollectionStorageService.State state)
        => _service.TryGetState(id, out state);

    public bool TryGetDefinition(string id, out ItemDefinition def)
        => database.TryGet(id, out def);

    private void OnApplicationPause(bool pause)
    {
        if (pause)
            _service.SaveIfDirty();
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus)
            _service.SaveIfDirty();
    }


    public IEnumerable<(ItemDefinition def, CollectionStorageService.State state)> GetAllItemsWithState()
    {
        foreach (var item in database.Items)
        {
            if (_service.TryGetState(item.Id, out var state))
            {
                yield return (item, state);
            }
        }
    }

}
