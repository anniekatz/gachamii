using UnityEngine;

[CreateAssetMenu(menuName = "Collection/Item Definition")]
public sealed class ItemDefinition : ScriptableObject
{
    [Header("Identity")]
    [SerializeField] private string id;

    [Header("Static data")]
    [SerializeField] private string displayName;
    [TextArea][SerializeField] private string description;
    [SerializeField] private int generation = 1;
    [SerializeField] private bool isSfw = true;
    [SerializeField] private int commonalityScore = 10;

    [Header("Sprites")]
    [SerializeField] private Sprite spriteNormal;
    [SerializeField] private Sprite spriteGrey;

    public string Id => id;
    public string DisplayName => displayName;
    public string Description => description;
    public int Generation => generation;
    public bool IsSfw => isSfw;
    public int CommonalityScore => commonalityScore;

    public Sprite Normal => spriteNormal;
    public Sprite Grey => spriteGrey;
}
