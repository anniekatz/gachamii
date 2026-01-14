using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Collider2D))]
public class MakeDroppedCapsuleClickable : MonoBehaviour
{
    [Header("Fires when capsule is tapped")]
    [SerializeField] private UnityEvent onTapped;

    [Header("Optional: explicit camera (defaults to Camera.main)")]
    [SerializeField] private Camera worldCamera;

    private Collider2D _selfCollider;
    private bool _armed;

    private void Awake()
    {
        _selfCollider = GetComponent<Collider2D>();

        if (worldCamera == null)
            worldCamera = Camera.main;

        if (worldCamera == null)
        {
            Debug.LogError("CapsuleTap: no camera assigned and no Camera.main found.");
            enabled = false;
            return;
        }

        // not clickable unless drop is finished
        SetArmed(false);
    }
    //capsule dropper calls
    public void SetArmed(bool value)
    {
        _armed = value;
        if (_selfCollider != null) _selfCollider.enabled = value;
    }

    public void HideSelf()
    {
        SetArmed(false);
        gameObject.SetActive(false);
    }

    private void Update()
    {
        if (!_armed) return;
        if (!TryGetPointerState(out PointerState pointer)) return;

        Vector3 world = ScreenToWorld(pointer.screenPosition);

        if (pointer.justPressed && _selfCollider.OverlapPoint(world))
        {
            SetArmed(false);     // prevent double taps
            onTapped?.Invoke();
        }
    }

    private Vector3 ScreenToWorld(Vector2 screenPos)
    {
        var v = new Vector3(screenPos.x, screenPos.y, 0f);
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

    // mouse or tap
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