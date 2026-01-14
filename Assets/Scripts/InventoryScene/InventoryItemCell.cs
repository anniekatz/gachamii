using TMPro;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UI;

public class InventoryItemCell : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Image itemImage;
    [SerializeField] private TextMeshProUGUI amountText;
    [SerializeField] public Button cellButton;
    [SerializeField] private AspectRatioFitter aspectFitter;

    private ItemDefinition _itemDef;
    private CollectionStorageService.State _state;

    public void Setup(ItemDefinition def, CollectionStorageService.State state)
    {
        _itemDef = def;
        _state = state;

        if (itemImage != null)
        {
            itemImage.sprite = state.seen ? def.Normal : def.Grey;
            itemImage.enabled = true;

            if (aspectFitter != null && itemImage.sprite != null)
            {
                float width = itemImage.sprite.rect.width;
                float height = itemImage.sprite.rect.height;
                aspectFitter.aspectRatio = width / height;
            }
        }

        if (amountText != null)
        {
            bool showAmount = state.amount > 0;
            if (amountText.gameObject.activeSelf != showAmount)
                amountText.gameObject.SetActive(showAmount);

            if (showAmount) amountText.text = $"x{state.amount}";
        }
    }

    public ItemDefinition GetItemDef() => _itemDef;
}