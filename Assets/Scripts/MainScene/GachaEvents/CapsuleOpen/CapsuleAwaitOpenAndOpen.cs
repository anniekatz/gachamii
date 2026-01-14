using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Collider2D))]
public sealed class CapsuleAwaitOpenAndOpen : MonoBehaviour
{
    [Header("Events")]
    [SerializeField] private UnityEngine.Events.UnityEvent onOpened;

    [Header("Pieces")]
    [SerializeField] private SpriteRenderer topCapsule;
    [SerializeField] private SpriteRenderer bottomCapsule;
    [SerializeField] private SpriteRenderer greyedItemWon;

    [Header("Masks (to disable on open)")]
    [SerializeField] private SpriteMask startMask;
    [SerializeField] private SpriteMask endMask;

    [Header("Camera (defaults to Camera.main)")]
    [SerializeField] private Camera worldCamera;

    [Header("Idle shake (awaiting open)")]
    [SerializeField] private float shakePosAmplitude = 0.02f;
    [SerializeField] private float shakeRotAmplitude = 2f;

    [Header("Patterned shake (shake, shake, pause...)")]
    [SerializeField] private float shakeStepDuration = 0.06f;
    [SerializeField] private float pauseDuration = 0.75f;
    [SerializeField] private int shakesPerBurst = 2;
    [SerializeField] private bool shakeLeftFirst = true;
    [SerializeField] private float maxRotMultiplier = 1f;

    [Header("Opening split distance computed from sprite sizes")]
    [SerializeField] private float splitClearancePadding = 0.05f;
    [SerializeField] private float splitMinWorldDistance = 0.35f;
    [SerializeField] private float splitMoveDuration = 0.6f;
    [SerializeField] private float splitHoldBeforeFade = 0.10f;
    [SerializeField] private float fadeDuration = 0.3f;               // duration of capsule fadeout

    [Header("Grey behavior during open")]
    [SerializeField] private float greyScaleDuration = 0.55f;
    [SerializeField] private float greyScaleStartDelay = 0.05f;
    [SerializeField, Range(0.1f, 1f)] private float greyTargetScreenWidth = 0.85f;
    [SerializeField, Range(0.1f, 1f)] private float greyTargetScreenHeight = 0.6f;

    private Collider2D _tapCollider;
    private bool _armed;

    private Coroutine _shakeRoutine;
    private Coroutine _openRoutine;

    private Vector3 _baseLocalPos;
    private Quaternion _baseLocalRot;

    private Vector3 _topLocalStart;
    private Vector3 _bottomLocalStart;
    private Vector3 _greyLocalStart;
    private Vector3 _greyScaleStart;

    private Color _topColorStart;
    private Color _bottomColorStart;
    private bool _colorsInitialized = false;

    private void Awake()
    {
        _tapCollider = GetComponent<Collider2D>();

        if (worldCamera == null)
            worldCamera = Camera.main;

        if (worldCamera == null)
        {
            Debug.LogError("[CapsuleAwaitOpenAndOpen] No camera assigned and no Camera.main found. Disabling.");
            enabled = false;
            return;
        }

        if (topCapsule != null) _topLocalStart = topCapsule.transform.localPosition;
        if (bottomCapsule != null) _bottomLocalStart = bottomCapsule.transform.localPosition;
        if (greyedItemWon != null)
        {
            _greyLocalStart = greyedItemWon.transform.localPosition;
            _greyScaleStart = greyedItemWon.transform.localScale;
        }

        if (topCapsule != null) _topColorStart = topCapsule.color;
        if (bottomCapsule != null) _bottomColorStart = bottomCapsule.color;

        _colorsInitialized = true;

        SetArmed(false);
    }

    public void BeginAwaitingOpen()
    {
        _baseLocalPos = transform.localPosition;
        _baseLocalRot = transform.localRotation;

        // bounce changed scales so get current
        if (topCapsule != null) _topLocalStart = topCapsule.transform.localPosition;
        if (bottomCapsule != null) _bottomLocalStart = bottomCapsule.transform.localPosition;
        if (greyedItemWon != null)
        {
            _greyLocalStart = greyedItemWon.transform.localPosition;
            _greyScaleStart = greyedItemWon.transform.localScale;
        }

        StartShake();
        SetArmed(true);
    }

    private void SetArmed(bool value)
    {
        _armed = value;
        if (_tapCollider != null) _tapCollider.enabled = value;
    }

    private void StartShake()
    {
        StopShake();
        _shakeRoutine = StartCoroutine(ShakeRoutine());
    }

    private void StopShake()
    {
        if (_shakeRoutine != null)
        {
            StopCoroutine(_shakeRoutine);
            _shakeRoutine = null;
        }

        // reset to base position
        transform.localPosition = _baseLocalPos;
        transform.localRotation = _baseLocalRot;
    }

    private IEnumerator ShakeRoutine()
    {
        Vector3 basePos = _baseLocalPos;
        Quaternion baseRot = _baseLocalRot;
        float dir = shakeLeftFirst ? -1f : 1f;

        while (true)
        {
            //shake
            for (int i = 0; i < shakesPerBurst; i++)
            {
                //left
                yield return ShakeStep(basePos, baseRot, dir);

                //right
                yield return ShakeStep(basePos, baseRot, -dir);

                //center
                yield return ShakeStep(basePos, baseRot, 0f);
            }

            //pause
            transform.localPosition = basePos;
            transform.localRotation = baseRot;
            yield return new WaitForSeconds(pauseDuration);
        }
    }

    private IEnumerator ShakeStep(Vector3 basePos, Quaternion baseRot, float sideSign)
    {
        Vector3 targetPos = basePos + new Vector3(sideSign * shakePosAmplitude, 0f, 0f);

        float rot = -sideSign * shakeRotAmplitude * maxRotMultiplier;
        Quaternion targetRot = baseRot * Quaternion.Euler(0f, 0f, rot);

        float elapsed = 0f;
        Vector3 startPos = transform.localPosition;
        Quaternion startRot = transform.localRotation;

        while (elapsed < shakeStepDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / shakeStepDuration);

            float e = t < 0.5f ? 2f * t * t : 1f - Mathf.Pow(-2f * t + 2f, 2f) / 2f;

            transform.localPosition = Vector3.LerpUnclamped(startPos, targetPos, e);
            transform.localRotation = Quaternion.SlerpUnclamped(startRot, targetRot, e);

            yield return null;
        }

        transform.localPosition = targetPos;
        transform.localRotation = targetRot;
    }


    private void Update()
    {
        if (!_armed) return;
        if (!TryGetPointerState(out var pointer)) return;
        if (!pointer.justPressed) return;

        Vector3 world = ScreenToWorld(pointer.screenPosition);

        if (_tapCollider.OverlapPoint(world))
        {
            //tapped capsule?
            SetArmed(false);
            StopShake();

            if (_openRoutine != null) StopCoroutine(_openRoutine);
            _openRoutine = StartCoroutine(OpenRoutine());
        }
    }

    private IEnumerator OpenRoutine()
    {
        if (startMask != null) startMask.gameObject.SetActive(false);
        if (endMask != null) endMask.gameObject.SetActive(false);

        transform.localPosition = _baseLocalPos;
        transform.localRotation = _baseLocalRot;

        Vector3 topStart = topCapsule.transform.localPosition;
        Vector3 bottomStart = bottomCapsule.transform.localPosition;

        float capsuleHalfH = Mathf.Max(topCapsule.bounds.size.y, bottomCapsule.bounds.size.y) * 0.5f;
        float greyHalfH = greyedItemWon.bounds.size.y * 0.5f;

        float requiredWorldDistance = capsuleHalfH + greyHalfH + splitClearancePadding;
        requiredWorldDistance = Mathf.Max(requiredWorldDistance, splitMinWorldDistance);

        float parentScaleY = Mathf.Abs(transform.lossyScale.y);
        if (parentScaleY < 0.0001f) parentScaleY = 1f;
        float localSplit = requiredWorldDistance / parentScaleY;

        Vector3 topEnd = topStart + Vector3.up * localSplit;
        Vector3 bottomEnd = bottomStart + Vector3.down * localSplit;

        float z = greyedItemWon.transform.position.z;

        float targetWorldWidth = GetVisibleWorldWidthAtZ(z) * greyTargetScreenWidth;
        float currentWorldWidth = greyedItemWon.bounds.size.x;
        float scaleByWidth = (currentWorldWidth > 0.0001f) ? (targetWorldWidth / currentWorldWidth) : 1f;

        float targetWorldHeight = GetVisibleWorldHeightAtZ(z) * greyTargetScreenHeight;
        float currentWorldHeight = greyedItemWon.bounds.size.y;
        float scaleByHeight = (currentWorldHeight > 0.0001f) ? (targetWorldHeight / currentWorldHeight) : 1f;

        float scaleFactor = Mathf.Min(scaleByWidth, scaleByHeight);

        Vector3 greyScale0 = greyedItemWon.transform.localScale;
        Vector3 greyScale1 = greyScale0 * scaleFactor;
        Vector3 greyLocalPos = greyedItemWon.transform.localPosition;

        Color topC0 = topCapsule.color;
        Color bottomC0 = bottomCapsule.color;

        float elapsed = 0f;
        float greyDelay = Mathf.Max(0f, greyScaleStartDelay);

        while (elapsed < splitMoveDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / splitMoveDuration);
            float e = 1f - Mathf.Pow(1f - t, 3f);

            topCapsule.transform.localPosition = Vector3.LerpUnclamped(topStart, topEnd, e);
            bottomCapsule.transform.localPosition = Vector3.LerpUnclamped(bottomStart, bottomEnd, e);

            greyedItemWon.transform.localPosition = greyLocalPos;
            if (elapsed >= greyDelay)
            {
                // scale up the item won
                float greyT = Mathf.Clamp01((elapsed - greyDelay) / splitMoveDuration);
                float ge = 1f - Mathf.Pow(1f - greyT, 3f);
                greyedItemWon.transform.localScale = Vector3.LerpUnclamped(greyScale0, greyScale1, ge);
            }

            yield return null;
        }

        topCapsule.transform.localPosition = topEnd;
        bottomCapsule.transform.localPosition = bottomEnd;
        if (splitHoldBeforeFade > 0f)
            yield return new WaitForSeconds(splitHoldBeforeFade);

        // fade out capsules after they split
        elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / fadeDuration);
            float e = t * t;

            var tc = topC0; tc.a = Mathf.Lerp(topC0.a, 0f, e); topCapsule.color = tc;
            var bc = bottomC0; bc.a = Mathf.Lerp(bottomC0.a, 0f, e); bottomCapsule.color = bc;

            yield return null;
        }

        // final alpha of 0
        var tcBackFinal = topCapsule.color; tcBackFinal.a = 0f; topCapsule.color = tcBackFinal;
        var bcFinal = bottomCapsule.color; bcFinal.a = 0f; bottomCapsule.color = bcFinal;

        greyedItemWon.enabled = true;

        Vector3 greyScaleStartNow = greyedItemWon.transform.localScale;
        elapsed = 0f;
        while (elapsed < greyScaleDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / greyScaleDuration);
            float e = 1f - Mathf.Pow(1f - t, 3f);

            greyedItemWon.transform.localScale = Vector3.LerpUnclamped(greyScaleStartNow, greyScale1, e);
            greyedItemWon.transform.localPosition = greyLocalPos;
            yield return null;
        }

        greyedItemWon.transform.localScale = greyScale1;
        greyedItemWon.transform.localPosition = greyLocalPos;

        onOpened?.Invoke();
        _openRoutine = null;

        _openRoutine = null;
    }

    public void ResetClosedVisuals()
    {
        if (_openRoutine != null)
        {
            StopCoroutine(_openRoutine);
            _openRoutine = null;
        }

        StopShake();
        SetArmed(false);

        if (topCapsule != null) topCapsule.transform.localPosition = _topLocalStart;
        if (bottomCapsule != null) bottomCapsule.transform.localPosition = _bottomLocalStart;

        if (greyedItemWon != null)
        {
            greyedItemWon.transform.localPosition = _greyLocalStart;
            greyedItemWon.transform.localScale = _greyScaleStart;

            var gc = greyedItemWon.color;
            gc.a = 1f;
            greyedItemWon.color = gc;
            greyedItemWon.enabled = true;
        }

        if (_colorsInitialized)
        {
            if (topCapsule != null) topCapsule.color = _topColorStart;
            if (bottomCapsule != null) bottomCapsule.color = _bottomColorStart;
        }
    }


    private float GetVisibleWorldWidthAtZ(float worldZ)
    {
        float depth = worldCamera.orthographic
            ? 0f
            : Mathf.Abs(worldCamera.transform.position.z - worldZ);

        Vector3 left = worldCamera.ViewportToWorldPoint(new Vector3(0f, 0.5f, depth));
        Vector3 right = worldCamera.ViewportToWorldPoint(new Vector3(1f, 0.5f, depth));
        return Mathf.Abs(right.x - left.x);
    }

    private float GetVisibleWorldHeightAtZ(float worldZ)
    {
        float depth = worldCamera.orthographic
            ? 0f
            : Mathf.Abs(worldCamera.transform.position.z - worldZ);

        Vector3 bottom = worldCamera.ViewportToWorldPoint(new Vector3(0.5f, 0f, depth));
        Vector3 top = worldCamera.ViewportToWorldPoint(new Vector3(0.5f, 1f, depth));
        return Mathf.Abs(top.y - bottom.y);
    }

    private Vector3 ScreenToWorld(Vector2 screenPos)
    {
        float depth = worldCamera.orthographic
            ? 0f
            : Mathf.Abs(worldCamera.transform.position.z - transform.position.z);

        var v = new Vector3(screenPos.x, screenPos.y, depth);
        var world = worldCamera.ScreenToWorldPoint(v);
        world.z = transform.position.z;
        return world;
    }

    private struct PointerState
    {
        public Vector2 screenPosition;
        public bool justPressed;
        public bool isPressed;
        public bool justReleased;
    }

    private bool TryGetPointerState(out PointerState state)
    {
        state = default;

        if (Touchscreen.current != null)
        {
            var touch = Touchscreen.current.primaryTouch;

            bool pressedThisFrame = touch.press.wasPressedThisFrame;
            bool pressed = touch.press.isPressed;
            bool releasedThisFrame = touch.press.wasReleasedThisFrame;

            if (pressed || pressedThisFrame || releasedThisFrame)
            {
                state.screenPosition = touch.position.ReadValue();
                state.justPressed = pressedThisFrame;
                state.isPressed = pressed;
                state.justReleased = releasedThisFrame;
                return true;
            }
        }

        if (Mouse.current != null)
        {
            bool pressedThisFrame = Mouse.current.leftButton.wasPressedThisFrame;
            bool pressed = Mouse.current.leftButton.isPressed;
            bool releasedThisFrame = Mouse.current.leftButton.wasReleasedThisFrame;

            if (pressed || pressedThisFrame || releasedThisFrame)
            {
                state.screenPosition = Mouse.current.position.ReadValue();
                state.justPressed = pressedThisFrame;
                state.isPressed = pressed;
                state.justReleased = releasedThisFrame;
                return true;
            }
        }

        return false;
    }
}
