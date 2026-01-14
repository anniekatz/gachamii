using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class PopupController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject dimBackground;
    [SerializeField] private CanvasGroup mainUICanvasGroup;
    [SerializeField] private Image[] dimWhenLocked;

    [Header("Sprite-based Elements (outside CanvasGroup system)")]
    [SerializeField] private SpriteRenderer draggableCoinRenderer;

    [Header("Settings")]
    [SerializeField] private bool pauseTimeWhenOpen = true;
    [SerializeField] private float dimmedAlpha = 0.4f;

    [Header("Events")]
    public UnityEvent OnPopupOpened;
    public UnityEvent OnPopupClosed;

    private float _previousTimeScale = 1f;
    private Color _originalCoinColor;
    private CoinDragger _coinDraggerScript;

    private void Awake()
    {
        if (draggableCoinRenderer != null)
        {
            _coinDraggerScript = draggableCoinRenderer.GetComponent<CoinDragger>();
        }
    }

    public void Open()
    {
        gameObject.SetActive(true);

        if (dimBackground != null)
            dimBackground.SetActive(true);

        if (mainUICanvasGroup != null)
        {
            mainUICanvasGroup.interactable = false;
            mainUICanvasGroup.blocksRaycasts = false;
        }

        if (dimWhenLocked != null)
        {
            foreach (var img in dimWhenLocked)
            {
                if (img != null)
                {
                    Color c = img.color;
                    c.a = dimmedAlpha;
                    img.color = c;
                }
            }
        }

        if (draggableCoinRenderer != null)
        {
            _originalCoinColor = draggableCoinRenderer.color;
            Color dimmed = _originalCoinColor;
            dimmed.a = dimmedAlpha;
            draggableCoinRenderer.color = dimmed;
        }

        if (_coinDraggerScript != null)
        {
            _coinDraggerScript.enabled = false;
        }

        if (pauseTimeWhenOpen)
        {
            _previousTimeScale = Time.timeScale;
            Time.timeScale = 0f;
        }

        OnPopupOpened?.Invoke();
    }

    public void Close()
    {
        gameObject.SetActive(false);

        if (dimBackground != null)
            dimBackground.SetActive(false);

        if (mainUICanvasGroup != null)
        {
            mainUICanvasGroup.interactable = true;
            mainUICanvasGroup.blocksRaycasts = true;
        }

        if (dimWhenLocked != null)
        {
            foreach (var img in dimWhenLocked)
            {
                if (img != null)
                {
                    Color c = img.color;
                    c.a = 1f;
                    img.color = c;
                }
            }
        }

        if (draggableCoinRenderer != null)
        {
            draggableCoinRenderer.color = _originalCoinColor;
        }

        if (_coinDraggerScript != null)
        {
            _coinDraggerScript.enabled = true;
        }

        if (pauseTimeWhenOpen)
        {
            Time.timeScale = _previousTimeScale;
        }

        OnPopupClosed?.Invoke();
    }
}