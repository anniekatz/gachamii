using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;


[RequireComponent(typeof(Collider2D))]
public class CoinDragger : MonoBehaviour
{
    [Header("Drop target (world-space 2D)")]
    [SerializeField] private Collider2D dropTarget;

    [Header("Event fired when dropped on target")]
    [SerializeField] private UnityEvent onDroppedOnTarget;

    [Header("Optional: explicit camera (defaults to Camera.main)")]
    [SerializeField] private Camera worldCamera;

    private Collider2D _selfCollider;
    private Vector3 _startPos;
    private bool _dragging;
    private Vector3 _dragOffset;

    private void Awake()
    {
        _selfCollider = GetComponent<Collider2D>();

        if (worldCamera == null)
            worldCamera = Camera.main;

        if (worldCamera == null)
        {
            Debug.LogError("CoinDragNewInput: no camera assigned and no Camera.main found.");
            enabled = false;
            return;
        }

        _startPos = transform.position;
    }

    private void Update()
    {
        if (!TryGetPointerState(out PointerState pointer))
            return;

        Vector3 world = ScreenToWorld(pointer.screenPosition);

        if (pointer.justPressed)
        {
            if (_selfCollider.OverlapPoint(world))
            {
                _dragging = true;
                _dragOffset = transform.position - world;
            }
        }

        if (_dragging && pointer.isPressed)
        {
            transform.position = world + _dragOffset;
        }

        if (_dragging && pointer.justReleased)
        {
            _dragging = false;

            bool droppedOnTarget = false;
            if (dropTarget != null)
            {
                droppedOnTarget = dropTarget.OverlapPoint(world);
            }

            transform.position = _startPos;

            if (droppedOnTarget)
            {
                onDroppedOnTarget?.Invoke();
            }
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

    public void SetStartPositionToCurrent()
    {
        _startPos = transform.position;
    }
}
