using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;

public sealed class GachaController : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] private ItemDatabase itemDatabase;

    [Header("Camera")]
    [SerializeField] private Camera worldCamera;

    [Header("Animation chain")]
    [SerializeField] private CapsuleDropAnimator dropAnimator;
    [SerializeField] private CapsuleBounceToView bounceToView;
    [SerializeField] private CapsuleAwaitOpenAndOpen awaitOpen;

    [Header("Item renderers (world)")]
    [SerializeField] private SpriteRenderer greyItemRenderer;
    [SerializeField] private SpriteRenderer normalItemRenderer;
    [SerializeField] private float crossFadeDuration = 0.35f;

    [Header("Lock coin while running")]
    [SerializeField] private CoinDragger coinDragger;
    [SerializeField] private SpriteRenderer coinSpriteRenderer;

    [Header("Lock UI while running (use MainUI)")]
    [SerializeField] private CanvasGroup uiLockGroup;
    [SerializeField] private Image[] dimWhenLocked;  // dim multiple ui images
    [SerializeField] private float dimmedAlpha = 0.4f;

    [Header("Reward Popup")]
    [SerializeField] private GameObject rewardPopup;
    [SerializeField] private CanvasGroup rewardPopupCanvasGroup;
    [SerializeField] private RectTransform popupImageTarget;
    [SerializeField] private TextMeshProUGUI rewardNameText;
    [SerializeField] private TextMeshProUGUI rewardDescriptionText;
    [SerializeField] private TextMeshProUGUI rewardOwnedText;
    [SerializeField] private TextMeshProUGUI rewardGenerationText;
    [SerializeField] private float shrinkToPopupDuration = 0.4f;
    [SerializeField] private float popupFadeDuration = 0.3f;

    [Header("New Item Animation")]
    //characters
    public GameObject newItem;
    public GameObject nickSprite;
    private NickExpressionController nickExpression;

    [Header("Dismiss animations")]
    [SerializeField] private WindowSlider windowSlider;
    [SerializeField] private ItemFlyToInventory itemFlyToInventory;

    [Header("CollectibleOddsModifier")]
    [SerializeField] private float latestGenMultiplier = 1.25f;
    [SerializeField] private float nsfwOddsMultiplier = 2.0f;
    [SerializeField] private int nsfwOddsStartHour = 20;
    [SerializeField] private int nsfwOddsEndHour = 23;

    private ItemDefinition _currentReward;
    private bool _canDismiss;
    private bool _isRunning;
    private bool _isNewItem = false;

    private void Awake()
    {
        if (worldCamera == null)
            worldCamera = Camera.main;

        if (itemDatabase == null)
            Debug.LogError("[GachaController] ItemDatabase is not assigned.", this);

        HideRewardPopup();

        if (bounceToView != null)
            bounceToView.ResetToStart();

        if (awaitOpen != null)
            awaitOpen.ResetClosedVisuals();

        PrepareSpritesForStart();
        SetLocked(false);

        _isNewItem = false;
        nickExpression = nickSprite.GetComponent<NickExpressionController>();
        newItem.SetActive(false);

    }

    public void StartPull()
    {
        if (_isRunning) return;

        if (dropAnimator == null || bounceToView == null || awaitOpen == null)
        {
            Debug.LogError("[GachaController] Missing animation chain references.", this);
            return;
        }

        if (itemDatabase == null || itemDatabase.Items == null || itemDatabase.Items.Count == 0)
        {
            Debug.LogError("[GachaController] ItemDatabase is missing/empty.", this);
            return;
        }

        if (CoinManager.Instance == null)
        {
            Debug.LogError("[GachaController] CoinManager.Instance is null.", this);
            return;
        }

        if (!CoinManager.Instance.TrySpendCoins(100))
            return;

        _isRunning = true;
        _canDismiss = false;
        SetLocked(true);

        HideRewardPopup();

        if (awaitOpen != null) awaitOpen.ResetClosedVisuals();
        if (bounceToView != null) bounceToView.ResetToStart();
        PrepareSpritesForStart();

        _currentReward = PickWeightedRandom(itemDatabase);

        if (CollectionManager.Instance != null)
            CollectionManager.Instance.AddItem(_currentReward.Id, 1, markSeen: true);

        ApplyRewardSprites(_currentReward);
        dropAnimator.StartCapsuleDropAnimator();
    }

    public void OnCapsuleOpened()
    {
        if (_currentReward == null) return;
        StartCoroutine(RevealRoutine(_currentReward));
    }

    private IEnumerator RevealRoutine(ItemDefinition def)
    {
        if (normalItemRenderer != null)
        {
            normalItemRenderer.sprite = def.Normal;
            normalItemRenderer.enabled = true;
            var c = normalItemRenderer.color; c.a = 0f; normalItemRenderer.color = c;
        }

        if (greyItemRenderer != null)
        {
            greyItemRenderer.sprite = def.Grey;
            greyItemRenderer.enabled = true;
            var c = greyItemRenderer.color; c.a = 1f; greyItemRenderer.color = c;
        }

        float dur = Mathf.Max(0.01f, crossFadeDuration);
        float elapsed = 0f;

        while (elapsed < dur)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / dur);

            if (greyItemRenderer != null)
            {
                var c = greyItemRenderer.color;
                c.a = Mathf.Lerp(1f, 0f, t);
                greyItemRenderer.color = c;
            }

            if (normalItemRenderer != null)
            {
                var c = normalItemRenderer.color;
                c.a = Mathf.Lerp(0f, 1f, t);
                normalItemRenderer.color = c;
            }

            yield return null;
        }

        if (greyItemRenderer != null)
        {
            var c = greyItemRenderer.color; c.a = 0f; greyItemRenderer.color = c;
        }

        if (normalItemRenderer != null)
        {
            var c = normalItemRenderer.color; c.a = 1f; normalItemRenderer.color = c;
        }

        yield return StartCoroutine(ShowPopupAndShrinkItem(def));

        _canDismiss = true;
    }

    private IEnumerator ShowPopupAndShrinkItem(ItemDefinition def)
    {
        if (rewardPopup == null || popupImageTarget == null || normalItemRenderer == null)
        {
            Debug.LogWarning("[GachaController] Missing popup references, skipping popup animation.");
            yield break;
        }

        if (worldCamera == null)
            worldCamera = Camera.main;


        if (rewardNameText != null) rewardNameText.text = def.DisplayName;
        if (rewardDescriptionText != null) rewardDescriptionText.text = def.Description;

        if (rewardGenerationText != null) rewardGenerationText.text = $"Gen {def.Generation}";


        if (rewardOwnedText != null)
        {
            int ownedCount = 0;
            if (CollectionManager.Instance != null &&
                CollectionManager.Instance.TryGetState(def.Id, out var state))
            {
                ownedCount = state.amount;
            }
            rewardOwnedText.text = $"Owned: {ownedCount}";

            if (ownedCount == 1 && newItem != null)
            {
                if (nickExpression != null)
                {
                    _isNewItem = true;
                }
            }
        }


        if (rewardPopupCanvasGroup != null)
            rewardPopupCanvasGroup.alpha = 0f;

        rewardPopup.SetActive(true);

        if (_isNewItem == true)
        {
            newItem.SetActive(true);
            nickExpression.StartNickSlowedWiggle(20.0f);
        }


        if (rewardPopupCanvasGroup != null)
        {
            float fadeElapsed = 0f;
            float fadeDur = popupFadeDuration * 0.5f;
            while (fadeElapsed < fadeDur)
            {
                fadeElapsed += Time.deltaTime;
                rewardPopupCanvasGroup.alpha = Mathf.Clamp01(fadeElapsed / fadeDur);
                yield return null;
            }
            rewardPopupCanvasGroup.alpha = 1f;
        }

        Vector3 startPos = normalItemRenderer.transform.position;
        Vector3 startScale = normalItemRenderer.transform.localScale;


        Vector3[] corners = new Vector3[4];
        popupImageTarget.GetWorldCorners(corners);
        Vector3 rectCenter = (corners[0] + corners[2]) / 2f;


        Vector2 screenCenter = RectTransformUtility.WorldToScreenPoint(worldCamera, rectCenter);
        Vector3 targetPos = worldCamera.ScreenToWorldPoint(new Vector3(screenCenter.x, screenCenter.y, worldCamera.WorldToScreenPoint(startPos).z));
        targetPos.z = startPos.z;


        Vector2 screenCorner0 = RectTransformUtility.WorldToScreenPoint(worldCamera, corners[0]);
        Vector2 screenCorner2 = RectTransformUtility.WorldToScreenPoint(worldCamera, corners[2]);
        float targetScreenWidth = Mathf.Abs(screenCorner2.x - screenCorner0.x);
        float targetScreenHeight = Mathf.Abs(screenCorner2.y - screenCorner0.y);

        Bounds spriteBounds = normalItemRenderer.bounds;
        Vector2 spriteScreenMin = worldCamera.WorldToScreenPoint(spriteBounds.min);
        Vector2 spriteScreenMax = worldCamera.WorldToScreenPoint(spriteBounds.max);
        float currentScreenWidth = Mathf.Abs(spriteScreenMax.x - spriteScreenMin.x);
        float currentScreenHeight = Mathf.Abs(spriteScreenMax.y - spriteScreenMin.y);

        float scaleX = (currentScreenWidth > 0.001f) ? targetScreenWidth / currentScreenWidth : 1f;
        float scaleY = (currentScreenHeight > 0.001f) ? targetScreenHeight / currentScreenHeight : 1f;
        float scaleFactor = Mathf.Min(scaleX, scaleY);

        Vector3 targetScale = startScale * scaleFactor;

        float elapsed = 0f;
        float dur = Mathf.Max(0.01f, shrinkToPopupDuration);

        while (elapsed < dur)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / dur);
            float eased = 1f - Mathf.Pow(1f - t, 3f);

            normalItemRenderer.transform.position = Vector3.Lerp(startPos, targetPos, eased);
            normalItemRenderer.transform.localScale = Vector3.Lerp(startScale, targetScale, eased);

            yield return null;
        }

        normalItemRenderer.transform.position = targetPos;
        normalItemRenderer.transform.localScale = targetScale;
    }

    private void Update()
    {
        if (!_canDismiss) return;

        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
        {
            _canDismiss = false;
            Dismiss();
            return;
        }

        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            _canDismiss = false;
            Dismiss();
            return;
        }
    }

    private void Dismiss()
    {
        StartCoroutine(DismissRoutine());
    }

    private IEnumerator DismissRoutine()
    {
        if (rewardPopupCanvasGroup != null)
        {
            float elapsed = 0f;
            float dur = popupFadeDuration;

            while (elapsed < dur)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / dur);
                rewardPopupCanvasGroup.alpha = Mathf.Lerp(1f, 0f, t);
                yield return null;
            }

            rewardPopupCanvasGroup.alpha = 0f;
        }

        HideRewardPopup();
        _isNewItem = false;
        newItem.SetActive(false);

        if (normalItemRenderer != null)
            normalItemRenderer.enabled = false;

        windowSlider.SlideDown();

        if (itemFlyToInventory != null && normalItemRenderer != null)
        {
            yield return StartCoroutine(itemFlyToInventory.FlyFromWorldSpriteCoroutine(normalItemRenderer));
        }

        if (awaitOpen != null) awaitOpen.ResetClosedVisuals();
        if (bounceToView != null) bounceToView.ResetToStart();
        PrepareSpritesForStart();

        _currentReward = null;
        _isRunning = false;
        SetLocked(false);
    }

    private void SetLocked(bool locked)
    {
        if (uiLockGroup != null)
        {
            uiLockGroup.interactable = !locked;
            uiLockGroup.blocksRaycasts = !locked;
        }

        if (coinDragger != null) coinDragger.enabled = !locked;
        if (coinSpriteRenderer != null) coinSpriteRenderer.enabled = !locked;

        if (dimWhenLocked != null)
        {
            foreach (var dimImage in dimWhenLocked)
            {
                Color c = dimImage.color;
                c.a = locked ? dimmedAlpha : 1f;
                dimImage.color = c;
            }
        }
    }

    private void ApplyRewardSprites(ItemDefinition def)
    {
        if (greyItemRenderer != null)
        {
            greyItemRenderer.sprite = def.Grey;
            var c = greyItemRenderer.color; c.a = 1f; greyItemRenderer.color = c;
            greyItemRenderer.enabled = true;
        }

        if (normalItemRenderer != null)
        {
            normalItemRenderer.sprite = def.Normal;
            var c = normalItemRenderer.color; c.a = 0f; normalItemRenderer.color = c;
            normalItemRenderer.enabled = false;
        }
    }

    private void PrepareSpritesForStart()
    {
        if (greyItemRenderer != null)
        {
            var c = greyItemRenderer.color; c.a = 1f; greyItemRenderer.color = c;
            greyItemRenderer.enabled = true;
        }

        if (normalItemRenderer != null)
        {
            var c = normalItemRenderer.color; c.a = 0f; normalItemRenderer.color = c;
            normalItemRenderer.enabled = false;
        }
    }

    private void HideRewardPopup()
    {
        if (rewardPopup != null) rewardPopup.SetActive(false);
        if (rewardPopupCanvasGroup != null) rewardPopupCanvasGroup.alpha = 1f;
        if (rewardNameText != null) rewardNameText.text = "";
        if (rewardDescriptionText != null) rewardDescriptionText.text = "";
        if (rewardOwnedText != null) rewardOwnedText.text = "";
        if (rewardGenerationText != null) rewardGenerationText.text = "";
    }

    //old
    //private static ItemDefinition PickWeightedRandom(ItemDatabase db)
    //{
    //    int totalWeight = 0;

    //    for (int i = 0; i < db.Items.Count; i++)
    //    {
    //        var def = db.Items[i];
    //        if (def == null) continue;
    //        int w = Mathf.Clamp(def.CommonalityScore, 1, 10);
    //        totalWeight += w;
    //    }

    //    int roll = Random.Range(0, totalWeight);

    //    for (int i = 0; i < db.Items.Count; i++)
    //    {
    //        var def = db.Items[i];
    //        if (def == null) continue;
    //        int w = Mathf.Clamp(def.CommonalityScore, 1, 10);
    //        roll -= w;
    //        if (roll < 0) return def;
    //    }

    //    return db.Items[db.Items.Count - 1];
    //}

    private ItemDefinition PickWeightedRandom(ItemDatabase db)
    {
        int currentHour = System.DateTime.Now.Hour;
        bool isNsfwTime = currentHour >= nsfwOddsStartHour && currentHour <= nsfwOddsEndHour;

        //find latest gen
        int latestGen = 0;
        for (int i = 0; i < db.Items.Count; i++)
        {
            var def = db.Items[i];
            if (def == null) continue;

            if (!def.IsSfw && !isNsfwTime)
                continue;

            if (def.Generation > latestGen)
                latestGen = def.Generation;
        }

        float totalWeight = 0f;

        for (int i = 0; i < db.Items.Count; i++)
        {
            var def = db.Items[i];
            if (def == null) continue;

            //only sfw during day
            if (!def.IsSfw && !isNsfwTime)
                continue;

            float w = Mathf.Clamp(def.CommonalityScore, 1, 10);

            // latest gen multiplier
            if (def.Generation == latestGen)
                w *= latestGenMultiplier;

            // nsfw multiplier
            if (!def.IsSfw && isNsfwTime)
                w *= nsfwOddsMultiplier;

            totalWeight += w;
        }

        if (totalWeight <= 0f)
        {
            for (int i = 0; i < db.Items.Count; i++)
            {
                if (db.Items[i] != null && db.Items[i].IsSfw)
                    return db.Items[i];
            }
            return db.Items[0];
        }

        float roll = Random.Range(0f, totalWeight);

        for (int i = 0; i < db.Items.Count; i++)
        {
            var def = db.Items[i];
            if (def == null) continue;
            if (!def.IsSfw && !isNsfwTime)
                continue;

            float w = Mathf.Clamp(def.CommonalityScore, 1, 10);

            if (def.Generation == latestGen)
                w *= latestGenMultiplier;

            if (!def.IsSfw && isNsfwTime)
                w *= nsfwOddsMultiplier;

            roll -= w;
            if (roll < 0f) return def;
        }

        // fallback
        return db.Items[db.Items.Count - 1];
    }
}