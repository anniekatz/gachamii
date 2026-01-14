using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ItemFlyToInventory : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private RectTransform inventoryButtonTarget;
    [SerializeField] private Image flyingItemImage;
    [SerializeField] private Canvas canvas;
    [SerializeField] private Camera worldCamera;

    [Header("Animation Settings")]
    [SerializeField] private float flyDuration = 0.5f;
    [SerializeField] private float startScale = 1f;
    [SerializeField] private float endScale = 0.2f;
    [SerializeField] private AnimationCurve moveCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    [SerializeField] private AnimationCurve scaleCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [Header("Arc Settings")]
    [SerializeField] private bool useArc = true;
    [SerializeField] private float arcHeight = 100f;

    [Header("Feedback")]
    [SerializeField] private InventoryButtonFeedback inventoryFeedback;

    private RectTransform _flyingItemRect;
    private Coroutine _flyRoutine;
    public bool IsFlying => _flyRoutine != null;

    private void Awake()
    {
        if (worldCamera == null)
            worldCamera = Camera.main;

        if (flyingItemImage != null)
        {
            _flyingItemRect = flyingItemImage.GetComponent<RectTransform>();
            flyingItemImage.gameObject.SetActive(false);
        }

        ValidateReferences();
    }

    private void ValidateReferences()
    {
        if (inventoryButtonTarget == null)
            Debug.LogError("[ItemFlyToInventory] inventoryButtonTarget is not assigned!", this);
        if (flyingItemImage == null)
            Debug.LogError("[ItemFlyToInventory] flyingItemImage is not assigned!", this);
        if (canvas == null)
            Debug.LogError("[ItemFlyToInventory] canvas is not assigned!", this);
        if (worldCamera == null)
            Debug.LogError("[ItemFlyToInventory] worldCamera is null (Camera.main not found)!", this);
    }

    public void FlyFromWorldSprite(SpriteRenderer sourceSprite)
    {
        flyingItemImage.sprite = sourceSprite.sprite;
        flyingItemImage.SetNativeSize();

        Debug.Log($"[ItemFlyToInventory] Sprite copied: {sourceSprite.sprite.name}");

        Vector3 worldPos = sourceSprite.transform.position;
        Vector2 screenPos = worldCamera.WorldToScreenPoint(worldPos);

        Debug.Log($"[ItemFlyToInventory] World pos: {worldPos}, Screen pos: {screenPos}");

        Vector2 canvasPos;
        RectTransform canvasRect = canvas.GetComponent<RectTransform>();

        bool success = RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            screenPos,
            canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera,
            out canvasPos
        );

        Debug.Log($"[ItemFlyToInventory] Canvas pos: {canvasPos}, Conversion success: {success}");

        float worldSpriteScale = sourceSprite.transform.lossyScale.x;
        Bounds spriteBounds = sourceSprite.sprite.bounds;
        float worldWidth = spriteBounds.size.x * worldSpriteScale;

        Vector3 leftEdge = worldCamera.WorldToScreenPoint(sourceSprite.transform.position - Vector3.right * (spriteBounds.size.x * worldSpriteScale * 0.5f));
        Vector3 rightEdge = worldCamera.WorldToScreenPoint(sourceSprite.transform.position + Vector3.right * (spriteBounds.size.x * worldSpriteScale * 0.5f));
        float screenWidth = Mathf.Abs(rightEdge.x - leftEdge.x);

        float nativeWidth = flyingItemImage.rectTransform.rect.width;
        float matchedScale = (nativeWidth > 0) ? (screenWidth / nativeWidth) : startScale;

        Debug.Log($"[ItemFlyToInventory] Matched scale: {matchedScale}");

        _flyingItemRect.anchoredPosition = canvasPos;
        _flyingItemRect.localScale = Vector3.one * matchedScale;
        flyingItemImage.gameObject.SetActive(true);

        Debug.Log("[ItemFlyToInventory] Flying image activated, starting coroutine");

        if (_flyRoutine != null) StopCoroutine(_flyRoutine);
        _flyRoutine = StartCoroutine(FlyRoutine(canvasPos, matchedScale));
    }

    public IEnumerator FlyFromWorldSpriteCoroutine(SpriteRenderer sourceSprite)
    {
        FlyFromWorldSprite(sourceSprite);

        while (IsFlying)
        {
            yield return null;
        }
    }

    private IEnumerator FlyRoutine(Vector2 startPos, float actualStartScale)
    {
        Vector2 endPos;
        RectTransform canvasRect = canvas.GetComponent<RectTransform>();

        Vector3 buttonWorldPos = inventoryButtonTarget.position;
        Vector2 buttonScreenPos = RectTransformUtility.WorldToScreenPoint(
            canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera,
            buttonWorldPos
        );

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            buttonScreenPos,
            canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera,
            out endPos
        );

        Debug.Log($"[ItemFlyToInventory] Flying from {startPos} to {endPos}");

        float actualEndScale = actualStartScale * endScale;

        Debug.Log($"[ItemFlyToInventory] Scale: {actualStartScale} -> {actualEndScale}");

        float elapsed = 0f;

        while (elapsed < flyDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / flyDuration);

            float moveT = moveCurve.Evaluate(t);
            float scaleT = scaleCurve.Evaluate(t);

            Vector2 pos = Vector2.Lerp(startPos, endPos, moveT);
            if (useArc)
            {
                float arcOffset = arcHeight * 4f * moveT * (1f - moveT);
                pos.y += arcOffset;
            }
            _flyingItemRect.anchoredPosition = pos;

            float scale = Mathf.Lerp(actualStartScale, actualEndScale, scaleT);
            _flyingItemRect.localScale = Vector3.one * scale;

            yield return null;
        }

        _flyingItemRect.anchoredPosition = endPos;
        _flyingItemRect.localScale = Vector3.one * actualEndScale;

        Debug.Log("[ItemFlyToInventory] animation complete, hiding flying image");

        flyingItemImage.gameObject.SetActive(false);

        if (inventoryFeedback != null)
        {
            Debug.Log("[ItemFlyToInventory] triggering inventory feedback");
            inventoryFeedback.PlayFeedback();
        }
        else
        {
            Debug.LogWarning("[ItemFlyToInventory] inventoryFeedback is null, skipping feedback");
        }

        _flyRoutine = null;
    }
}