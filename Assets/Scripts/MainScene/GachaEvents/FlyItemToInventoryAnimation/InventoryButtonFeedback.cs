using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventoryButtonFeedback : MonoBehaviour
{
    [Header("Button to Animate")]
    [SerializeField] private RectTransform buttonTransform;

    [Header("Pulse Settings")]
    [SerializeField] private float pulseDuration = 0.3f;
    [SerializeField] private float pulseScale = 1.25f;
    [SerializeField] private int pulseCount = 2;

    [Header("Floating Text")]
    [SerializeField] private TextMeshProUGUI floatingText;
    [SerializeField] private float textRiseDuration = 0.8f;
    [SerializeField] private float textRiseDistance = 50f;
    [SerializeField] private float textFadeDelay = 0.3f;

    [Header("Optional Glow/Flash")]
    [SerializeField] private Image glowImage;                     // meh..
    [SerializeField] private float glowDuration = 0.4f;

    private Vector3 _originalScale;
    private Coroutine _pulseRoutine;
    private Coroutine _textRoutine;
    private Coroutine _glowRoutine;

    private void Awake()
    {
        if (buttonTransform != null)
            _originalScale = buttonTransform.localScale;

        if (floatingText != null)
            floatingText.gameObject.SetActive(false);

        if (glowImage != null)
            glowImage.gameObject.SetActive(false);
    }

    public void PlayFeedback()
    {
        PlayPulse();
        PlayFloatingText();
        PlayGlow();
    }

    public void PlayPulse()
    {
        if (buttonTransform == null) return;

        if (_pulseRoutine != null) StopCoroutine(_pulseRoutine);
        _pulseRoutine = StartCoroutine(PulseRoutine());
    }

    public void PlayFloatingText(string text = "+1")
    {
        if (floatingText == null) return;

        if (_textRoutine != null) StopCoroutine(_textRoutine);
        _textRoutine = StartCoroutine(FloatingTextRoutine(text));
    }

    public void PlayGlow()
    {
        if (glowImage == null) return;

        if (_glowRoutine != null) StopCoroutine(_glowRoutine);
        _glowRoutine = StartCoroutine(GlowRoutine());
    }

    private IEnumerator PulseRoutine()
    {
        float singlePulseDur = pulseDuration / pulseCount;

        for (int i = 0; i < pulseCount; i++)
        {
            float elapsed = 0f;
            float halfDur = singlePulseDur * 0.5f;

            while (elapsed < halfDur)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / halfDur);
                float e = EaseOutQuad(t);
                buttonTransform.localScale = Vector3.Lerp(_originalScale, _originalScale * pulseScale, e);
                yield return null;
            }

            elapsed = 0f;
            while (elapsed < halfDur)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / halfDur);
                float e = EaseInQuad(t);
                buttonTransform.localScale = Vector3.Lerp(_originalScale * pulseScale, _originalScale, e);
                yield return null;
            }
        }

        buttonTransform.localScale = _originalScale;
        _pulseRoutine = null;
    }

    private IEnumerator FloatingTextRoutine(string text)
    {
        floatingText.text = text;
        floatingText.gameObject.SetActive(true);

        RectTransform textRect = floatingText.GetComponent<RectTransform>();
        Vector2 startPos = textRect.anchoredPosition;
        Vector2 endPos = startPos + Vector2.up * textRiseDistance;

        Color c = floatingText.color;
        c.a = 1f;
        floatingText.color = c;

        float elapsed = 0f;

        while (elapsed < textRiseDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / textRiseDuration);

            float moveT = EaseOutQuad(t);
            textRect.anchoredPosition = Vector2.Lerp(startPos, endPos, moveT);

            if (elapsed > textFadeDelay)
            {
                float fadeT = (elapsed - textFadeDelay) / (textRiseDuration - textFadeDelay);
                c.a = Mathf.Lerp(1f, 0f, fadeT);
                floatingText.color = c;
            }

            yield return null;
        }

        floatingText.gameObject.SetActive(false);

        textRect.anchoredPosition = startPos;
        c.a = 1f;
        floatingText.color = c;

        _textRoutine = null;
    }

    private IEnumerator GlowRoutine()
    {
        glowImage.gameObject.SetActive(true);

        Color c = glowImage.color;
        float elapsed = 0f;
        float halfDur = glowDuration * 0.5f;

        while (elapsed < halfDur)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / halfDur);
            c.a = Mathf.Lerp(0f, 1f, t);
            glowImage.color = c;
            yield return null;
        }

        elapsed = 0f;
        while (elapsed < halfDur)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / halfDur);
            c.a = Mathf.Lerp(1f, 0f, t);
            glowImage.color = c;
            yield return null;
        }

        c.a = 0f;
        glowImage.color = c;
        glowImage.gameObject.SetActive(false);

        _glowRoutine = null;
    }

    private float EaseOutQuad(float t) => 1f - (1f - t) * (1f - t);
    private float EaseInQuad(float t) => t * t;
}