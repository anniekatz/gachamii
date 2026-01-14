using System;
using System.Collections.Generic;

[Serializable]
public sealed class CollectionSave
{
    public int version = 1;
    public List<Entry> entries = new();
}

[Serializable]
public sealed class Entry
{
    public string id;
    public int amount;
    public bool seen;
}
