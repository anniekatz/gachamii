using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class CapsuleBounceToView : MonoBehaviour
{
    [Header("Capsule Root (moves as one object)")]
    [SerializeField] private Transform capsuleRoot; // capsule + items

    [Header("Sprites to Animate (scales only)")]
    [SerializeField] private SpriteRenderer topCapsule;
    [SerializeField] private SpriteRenderer bottomCapsule;
    [SerializeField] private SpriteRenderer greyedItemWon;

    [Header("Sprite Masks")]
    [SerializeField] private SpriteMask startMask;
    [SerializeField] private SpriteMask endMask;

    [Header("Animation Settings")]
    [SerializeField] private float duration = 0.6f;
    [SerializeField] private float bounceOvershoot = 1.3f;
    [SerializeField] private float startDelay = 0f;
    [SerializeField] private float padding = 0.9f;

    [Header("Item Won Settings")]
    [SerializeField, Range(0f, 1f)] private float itemScaleMultiplier = 0.7f;

    [Header("Easing Type")]
    [SerializeField] private EaseType easeType = EaseType.EaseOutBack;

    [Header("Events")]
    [SerializeField] private UnityEvent onArrivedAtEnd;

    [SerializeField] private GameObject capsuleToBounceIn;

    public enum EaseType
    {
        EaseOutBack,
        EaseOutElastic,
        EaseOutBounce,
        EaseInOutBack
    }

    private SpriteRenderer[] sprites;
    private Vector3 startPosition;
    private Vector3 endPosition;
    private float[] startScales;
    private float[] endScales;

    private Coroutine _playRoutine;

    private void Awake()
    {
        sprites = new SpriteRenderer[] { topCapsule, bottomCapsule, greyedItemWon };
        startScales = new float[sprites.Length];
        endScales = new float[sprites.Length];

        if (capsuleRoot == null)
            Debug.LogError("[CapsuleBounceToView] capsuleRoot is not assigned.");
    }

    public bool IsPlaying => _playRoutine != null;

    [ContextMenu("Play Animation")]
    public void PlayAnimation()
    {
        if (_playRoutine != null) return;
        _playRoutine = StartCoroutine(AnimateBounceWrapper());
    }

    private IEnumerator AnimateBounceWrapper()
    {
        yield return AnimateBounce();
        _playRoutine = null;
        onArrivedAtEnd?.Invoke();
    }

    [ContextMenu("Reset to Start")]
    public void ResetToStart()
    {
        if (capsuleToBounceIn != null) capsuleToBounceIn.SetActive(false);

        CalculateScales();

        if (capsuleRoot != null)
            capsuleRoot.position = startPosition;

        for (int i = 0; i < sprites.Length; i++)
        {
            if (sprites[i] != null)
            {
                sprites[i].transform.localScale = Vector3.one * startScales[i];
                sprites[i].gameObject.SetActive(true);
            }
            
        }

        if (startMask != null) startMask.gameObject.SetActive(true);
        if (endMask != null) endMask.gameObject.SetActive(false);
        if (capsuleRoot != null) capsuleRoot.gameObject.SetActive(true);
    }

    private void CalculateScales()
    {
        if (topCapsule == null || topCapsule.sprite == null)
            return;

        startPosition = startMask.transform.position;
        endPosition = endMask.transform.position;

        Bounds startBounds = GetMaskBounds(startMask);
        Bounds endBounds = GetMaskBounds(endMask);

        float capsuleStartScale = CalculateFitScale(topCapsule, startBounds);
        float capsuleEndScale = CalculateFitScale(topCapsule, endBounds);

        Bounds capsuleBounds = topCapsule.sprite.bounds;
        Vector2 capsuleStartSize = capsuleBounds.size * capsuleStartScale;
        Vector2 capsuleEndSize = capsuleBounds.size * capsuleEndScale;

        for (int i = 0; i < sprites.Length; i++)
        {
            if (sprites[i] == null || sprites[i].sprite == null)
                continue;

            if (sprites[i] == greyedItemWon)
            {
                Bounds greyedItemWonBounds = greyedItemWon.sprite.bounds;

                float targetStartWidth = capsuleStartSize.x * itemScaleMultiplier;
                float targetStartHeight = capsuleStartSize.y * itemScaleMultiplier;
                float targetEndWidth = capsuleEndSize.x * itemScaleMultiplier;
                float targetEndHeight = capsuleEndSize.y * itemScaleMultiplier;

                float startScaleX = targetStartWidth / greyedItemWonBounds.size.x;
                float startScaleY = targetStartHeight / greyedItemWonBounds.size.y;
                float endScaleX = targetEndWidth / greyedItemWonBounds.size.x;
                float endScaleY = targetEndHeight / greyedItemWonBounds.size.y;

                startScales[i] = Mathf.Min(startScaleX, startScaleY);
                endScales[i] = Mathf.Min(endScaleX, endScaleY);
            }
            else
            {
                startScales[i] = capsuleStartScale;
                endScales[i] = capsuleEndScale;
            }
        }
    }

    private Bounds GetMaskBounds(SpriteMask mask)
    {
        if (mask == null || mask.sprite == null)
            return new Bounds(Vector3.zero, Vector3.one);

        Bounds spriteBounds = mask.sprite.bounds;
        Vector3 maskScale = mask.transform.lossyScale;

        return new Bounds(
            mask.transform.position,
            new Vector3(
                spriteBounds.size.x * maskScale.x,
                spriteBounds.size.y * maskScale.y,
                spriteBounds.size.z * maskScale.z
            )
        );
    }

    private float CalculateFitScale(SpriteRenderer sprite, Bounds targetBounds)
    {
        if (sprite == null || sprite.sprite == null)
            return 1f;

        Bounds spriteBounds = sprite.sprite.bounds;
        float scaleX = targetBounds.size.x / spriteBounds.size.x;
        float scaleY = targetBounds.size.y / spriteBounds.size.y;
        return Mathf.Min(scaleX, scaleY) * padding;
    }

    private IEnumerator AnimateBounce()
    {
        if (capsuleRoot == null)
            yield break;

        // hide in initial state
        capsuleToBounceIn.SetActive(false);
        capsuleRoot.gameObject.SetActive(false);
        if (startMask != null) startMask.gameObject.SetActive(false);
        if (endMask != null) endMask.gameObject.SetActive(false);

        CalculateScales();

        // Set initial state
        capsuleRoot.position = startPosition;
        for (int i = 0; i < sprites.Length; i++)
        {
            if (sprites[i] != null)
                sprites[i].transform.localScale = Vector3.one * startScales[i];
        }

        yield return null;

        // show masks and capsule
        if (startMask != null) startMask.gameObject.SetActive(true);
        if (endMask != null) endMask.gameObject.SetActive(true);
        capsuleRoot.gameObject.SetActive(true);
        capsuleToBounceIn.SetActive(true);

        if (startDelay > 0f)
            yield return new WaitForSeconds(startDelay);

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float easedT = ApplyEasing(t);

            capsuleRoot.position = Vector3.LerpUnclamped(startPosition, endPosition, easedT);

            for (int i = 0; i < sprites.Length; i++)
            {
                if (sprites[i] == null) continue;

                float currentScale = Mathf.LerpUnclamped(startScales[i], endScales[i], easedT);
                sprites[i].transform.localScale = Vector3.one * currentScale;
            }

            yield return null;
        }

        capsuleRoot.position = endPosition;
        for (int i = 0; i < sprites.Length; i++)
        {
            if (sprites[i] != null)
                sprites[i].transform.localScale = Vector3.one * endScales[i];
        }

        // start mask not needed
        if (startMask != null) startMask.gameObject.SetActive(false);
    }

    private float ApplyEasing(float t)
    {
        return easeType switch
        {
            EaseType.EaseOutBack => EaseOutBack(t),
            EaseType.EaseOutElastic => EaseOutElastic(t),
            EaseType.EaseOutBounce => EaseOutBounce(t),
            EaseType.EaseInOutBack => EaseInOutBack(t),
            _ => t
        };
    }

    private float EaseOutBack(float t)
    {
        float c1 = bounceOvershoot;
        float c3 = c1 + 1f;
        return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
    }

    private float EaseInOutBack(float t)
    {
        float c1 = bounceOvershoot;
        float c2 = c1 * 1.525f;

        if (t < 0.5f)
            return (Mathf.Pow(2f * t, 2f) * ((c2 + 1f) * 2f * t - c2)) / 2f;

        return (Mathf.Pow(2f * t - 2f, 2f) * ((c2 + 1f) * (t * 2f - 2f) + c2) + 2f) / 2f;
    }

    private float EaseOutElastic(float t)
    {
        if (t == 0f) return 0f;
        if (t == 1f) return 1f;

        float p = 0.3f;
        float s = p / 4f;
        return Mathf.Pow(2f, -10f * t) * Mathf.Sin((t - s) * (2f * Mathf.PI) / p) + 1f;
    }

    private float EaseOutBounce(float t)
    {
        float n1 = 7.5625f;
        float d1 = 2.75f;

        if (t < 1f / d1) return n1 * t * t;
        if (t < 2f / d1) { t -= 1.5f / d1; return n1 * t * t + 0.75f; }
        if (t < 2.5f / d1) { t -= 2.25f / d1; return n1 * t * t + 0.9375f; }

        t -= 2.625f / d1;
        return n1 * t * t + 0.984375f;
    }
}
