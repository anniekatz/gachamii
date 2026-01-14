using System.Collections;
using TMPro;
using UnityEngine;

public class CoinDisplay : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TextMeshProUGUI coinText;
    [SerializeField] private RectTransform coinContainer;

    [Header("Floating Text")]
    [SerializeField] private GameObject floatingTextPrefab;
    [SerializeField] private Transform floatingTextSpawnPoint;

    [Header("Display Settings")]
    [SerializeField] private string format = "{0:N0}";

    private float bounceDuration = 0.4f;
    private float countUpDuration = 0.5f;
    private bool useCountUpAnimation = true;

    private float floatDistance = 50f;
    private float floatDuration = 1f;
    private Color gainColor = new Color(0.2f, 0.8f, 0.2f);
    private Color lossColor = new Color(0.9f, 0.3f, 0.3f);

    private int _displayedCoins;
    private Coroutine _countCoroutine;
    private Coroutine _bounceCoroutine;
    private Vector3 _originalScale;

    private void Awake()
    {
        if (coinContainer == null)
            coinContainer = GetComponent<RectTransform>();

        _originalScale = coinContainer.localScale;
    }

    private void Start()
    {
        if (CoinManager.Instance != null)
        {
            _displayedCoins = CoinManager.Instance.Coins;
            UpdateDisplayImmediate(_displayedCoins);

            CoinManager.Instance.OnCoinsChanged.AddListener(OnCoinsChanged);
        }
    }

    private void OnDestroy()
    {
        if (CoinManager.Instance != null)
        {
            CoinManager.Instance.OnCoinsChanged.RemoveListener(OnCoinsChanged);
        }
    }

    private void OnCoinsChanged(int newTotal)
    {
        int difference = newTotal - _displayedCoins;

        if (difference == 0) return;

        if (floatingTextPrefab != null)
        {
            ShowFloatingText(difference);
        }

        if (useCountUpAnimation)
        {
            AnimateCountUp(_displayedCoins, newTotal);
        }
        else
        {
            UpdateDisplayImmediate(newTotal);
        }

        PlayBounceAnimation();

        _displayedCoins = newTotal;
    }

    private void UpdateDisplayImmediate(int coins)
    {
        if (coinText != null)
        {
            coinText.text = FormatCoins(coins);
        }
    }

    private string FormatCoins(int coins)
    {
        if (coins >= 1_000_000_000)
        {
            float billions = coins / 1_000_000_000f;
            return FormatAbbreviated(billions, "B");
        }
        else if (coins >= 1_000_000)
        {
            float millions = coins / 1_000_000f;
            return FormatAbbreviated(millions, "M");
        }
        else if (coins >= 100_000)
        {
            float thousands = coins / 1_000f;
            return FormatAbbreviated(thousands, "k");
        }
        else
        {
            return string.Format(format, coins);
        }
    }

    private string FormatAbbreviated(float value, string suffix)
    {
        if (Mathf.Approximately(value % 1f, 0f))
        {
            return $"{value:0}{suffix}";
        }
        else
        {
            return $"{value:0.#}{suffix}";
        }
    }

    #region Count Up Animation

    private void AnimateCountUp(int from, int to)
    {
        if (_countCoroutine != null)
            StopCoroutine(_countCoroutine);

        _countCoroutine = StartCoroutine(CountUpCoroutine(from, to));
    }

    private IEnumerator CountUpCoroutine(int from, int to)
    {
        float elapsed = 0f;

        while (elapsed < countUpDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / countUpDuration);
            t = 1f - Mathf.Pow(1f - t, 3f);

            int current = Mathf.RoundToInt(Mathf.Lerp(from, to, t));
            UpdateDisplayImmediate(current);

            yield return null;
        }

        UpdateDisplayImmediate(to);
        _countCoroutine = null;
    }

    #endregion

    #region Bounce Animation

    private void PlayBounceAnimation()
    {
        if (_bounceCoroutine != null)
        {
            StopCoroutine(_bounceCoroutine);
            coinContainer.localScale = _originalScale;
        }
        _bounceCoroutine = StartCoroutine(BounceCoroutine());
    }

    private IEnumerator BounceCoroutine()
    {
        float elapsed = 0f;

        while (elapsed < bounceDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / bounceDuration;

            float scale = EaseOutElastic(t);
            coinContainer.localScale = _originalScale * scale;

            yield return null;
        }

        coinContainer.localScale = _originalScale;
        _bounceCoroutine = null;
    }

    private float EaseOutElastic(float t)
    {
        if (t <= 0f) return 1f;
        if (t >= 1f) return 1f;

        float p = 0.4f;
        float a = 0.2f;

        return 1f + a * Mathf.Pow(2f, -10f * t) * Mathf.Sin((t - p / 4f) * (2f * Mathf.PI) / p);
    }

    #endregion

    #region Floating Text

    private void ShowFloatingText(int amount)
    {
        if (floatingTextPrefab == null) return;

        Transform spawnPoint = floatingTextSpawnPoint != null ? floatingTextSpawnPoint : transform;
        GameObject floatingObj = Instantiate(floatingTextPrefab, spawnPoint.position, Quaternion.identity, transform.parent);

        TextMeshProUGUI floatingTMP = floatingObj.GetComponent<TextMeshProUGUI>();
        if (floatingTMP != null)
        {
            string prefix = amount > 0 ? "+" : "";
            floatingTMP.text = prefix + amount.ToString("N0");
            floatingTMP.color = amount > 0 ? gainColor : lossColor;
        }

        StartCoroutine(AnimateFloatingText(floatingObj));
    }

    private IEnumerator AnimateFloatingText(GameObject floatingObj)
    {
        if (floatingObj == null) yield break;

        RectTransform rect = floatingObj.GetComponent<RectTransform>();
        TextMeshProUGUI tmp = floatingObj.GetComponent<TextMeshProUGUI>();

        if (rect == null || tmp == null)
        {
            Destroy(floatingObj);
            yield break;
        }

        Vector3 startPos = rect.anchoredPosition;
        Vector3 endPos = startPos + Vector3.up * floatDistance;
        Color startColor = tmp.color;
        Color endColor = new Color(startColor.r, startColor.g, startColor.b, 0f);

        rect.localScale = Vector3.zero;

        float elapsed = 0f;
        float popDuration = 0.15f;

        while (elapsed < popDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / popDuration);
            float scale = EaseOutBack(t);
            rect.localScale = Vector3.one * scale;
            yield return null;
        }
        rect.localScale = Vector3.one;

        elapsed = 0f;
        while (elapsed < floatDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / floatDuration);
            float easeT = 1f - Mathf.Pow(1f - t, 2f);

            rect.anchoredPosition = Vector3.Lerp(startPos, endPos, easeT);
            tmp.color = Color.Lerp(startColor, endColor, t);

            yield return null;
        }

        Destroy(floatingObj);
    }

    private float EaseOutBack(float t)
    {
        float c = 1.70158f;
        return 1f + (c + 1f) * Mathf.Pow(t - 1f, 3f) + c * Mathf.Pow(t - 1f, 2f);
    }

    #endregion
}