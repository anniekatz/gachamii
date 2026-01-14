using System;
using UnityEngine;

public enum FitMode
{
    Cover,
    FitWidthCropHeight,
    FitHeightCropWidth
}

[RequireComponent(typeof(SpriteRenderer))]
public class FitBackgroundToScreen : MonoBehaviour
{
    [Header("Time-of-day Background Sprites")]
    [SerializeField] private Sprite dawnSprite;
    [SerializeField] private Sprite morningSprite;
    [SerializeField] private Sprite afternoonSprite;
    [SerializeField] private Sprite sunsetSprite;
    [SerializeField] private Sprite nightSprite;

    [Header("Time Ranges (24h, inclusive start)")]
    [Range(0, 23)] public int dawnStartHour = 5;
    [Range(0, 23)] public int morningStartHour = 8;
    [Range(0, 23)] public int afternoonStartHour = 15;
    [Range(0, 23)] public int sunsetStartHour = 18;
    [Range(0, 23)] public int nightStartHour = 20;

    public FitMode fitMode = FitMode.FitWidthCropHeight;

    private SpriteRenderer _sr;

    private void Awake()
    {
        _sr = GetComponent<SpriteRenderer>();
    }

    private void OnEnable()
    {
        ApplyBackgroundForLocalTime();

        ApplyFit();
    }

    public void ApplyBackgroundForLocalTime()
    {
        if (_sr == null) _sr = GetComponent<SpriteRenderer>();

        int hour = DateTime.Now.Hour;

        Sprite chosen = GetSpriteForHour(hour);
        if (chosen != null)
        {
            _sr.sprite = chosen;
        }
        else
        {
            _sr.sprite = afternoonSprite; //fallback
            Debug.LogWarning($"FitBackgroundToScreen: No sprite configured for hour {hour}, leaving current sprite.");
        }
    }

    private Sprite GetSpriteForHour(int hour)
    {
        if (IsHourInRange(hour, dawnStartHour, morningStartHour))
        {
            return dawnSprite;
        }

        if (IsHourInRange(hour, morningStartHour, afternoonStartHour))
        {
            return morningSprite;
        }

        if (IsHourInRange(hour, afternoonStartHour, sunsetStartHour))
        {
            return afternoonSprite;
        }

        if (IsHourInRange(hour, sunsetStartHour, nightStartHour))
        {
            return sunsetSprite;
        }

        if (IsHourInWrappedRange(hour, nightStartHour, dawnStartHour))
        {
            return nightSprite;
        }

        return afternoonSprite;
    }

    private bool IsHourInRange(int hour, int startInclusive, int endExclusive)
    {
        return hour >= startInclusive && hour < endExclusive;
    }

    private bool IsHourInWrappedRange(int hour, int startInclusive, int endExclusive)
    {
        if (startInclusive < endExclusive)
        {
            return hour >= startInclusive && hour < endExclusive;
        }

        return hour >= startInclusive || hour < endExclusive;
    }

    void ApplyFit()
    {
        var cam = Camera.main;

        if (_sr == null || _sr.sprite == null || cam == null)
            return;

        transform.localScale = Vector3.one;

        float spriteWidth = _sr.sprite.bounds.size.x;
        float spriteHeight = _sr.sprite.bounds.size.y;

        float worldScreenHeight = cam.orthographicSize * 2f;
        float worldScreenWidth = worldScreenHeight * Screen.width / Screen.height;

        float aspectYoverX = worldScreenHeight / worldScreenWidth;

        FitMode effectiveMode = fitMode;

        if (effectiveMode == FitMode.FitWidthCropHeight || effectiveMode == FitMode.Cover)
        {
            if (aspectYoverX >= 1.75f)
            {
                effectiveMode = FitMode.FitWidthCropHeight;
            }
            else
            {
                effectiveMode = FitMode.Cover;
            }
        }

        float scaleX = worldScreenWidth / spriteWidth;
        float scaleY = worldScreenHeight / spriteHeight;

        float scale = 1f;

        switch (effectiveMode)
        {
            case FitMode.Cover:
                scale = Mathf.Max(scaleX, scaleY);
                break;

            case FitMode.FitWidthCropHeight:
                scale = scaleX;
                break;

            case FitMode.FitHeightCropWidth:
                scale = scaleY;
                break;
        }

        transform.localScale = new Vector3(scale, scale, 1f);
    }
}
