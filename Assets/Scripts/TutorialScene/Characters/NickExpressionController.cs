using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public sealed class NickExpressionController : MonoBehaviour
{
    [Header("Expression Sprites")]
    public Sprite nkDefault;
    public Sprite nkSurprised;
    public Sprite nkLeft;
    public Sprite nkRight;
    public Sprite nkBoth;
    public Sprite nkBothClosed;
    public Sprite nkAngry;
    public Sprite nkConfused;

    private Image _image;
    private Coroutine _wiggleCoroutine;

    private void Awake()
    {
        _image = GetComponent<Image>();
        _image.preserveAspect = true;

        if (nkDefault != null)
            _image.sprite = nkDefault;
    }

    private void SetSprite(Sprite s)
    {
        if (s == null || _image == null) return;
        _image.sprite = s;
    }

    public void SetNickDefault() => SetSprite(nkDefault);
    public void SetNickSurprised() => SetSprite(nkSurprised);
    public void SetNickLeft() => SetSprite(nkLeft);
    public void SetNickRight() => SetSprite(nkRight);
    public void SetNickBoth() => SetSprite(nkBoth);
    public void SetNickBothClosed() => SetSprite(nkBothClosed);
    public void SetNickAngry() => SetSprite(nkAngry);
    public void SetNickConfused() => SetSprite(nkConfused);

    public void StartNickWiggle(float totalDurationSeconds)
    {
        if (_wiggleCoroutine != null) StopCoroutine(_wiggleCoroutine);
        _wiggleCoroutine = StartCoroutine(WiggleCoroutine(totalDurationSeconds));
    }

    public void StartNickSlowedWiggle(float totalDurationSeconds)
    {
        if (_wiggleCoroutine != null) StopCoroutine(_wiggleCoroutine);
        _wiggleCoroutine = StartCoroutine(SlowedWiggleCoroutine(totalDurationSeconds));
    }


    public void StartNickIndefiniteWiggle()
    {
        if (_wiggleCoroutine != null) StopCoroutine(_wiggleCoroutine);
        _wiggleCoroutine = StartCoroutine(IndefiniteWiggleCoroutine());
    }

    private IEnumerator WiggleCoroutine(float totalDurationSeconds)
    {
        float elapsed = 0f;
        bool isLeft = true;
        const float frameDuration = 0.25f;

        while (elapsed < totalDurationSeconds)
        {
            SetSprite(isLeft ? nkLeft : nkRight);
            isLeft = !isLeft;

            yield return new WaitForSeconds(frameDuration);
            elapsed += frameDuration;
        }

        SetSprite(nkDefault);
        _wiggleCoroutine = null;
    }


    private IEnumerator SlowedWiggleCoroutine(float totalDurationSeconds)
    {
        float elapsed = 0f;
        bool isLeft = true;
        const float frameDuration = 0.35f;

        while (elapsed < totalDurationSeconds)
        {
            SetSprite(isLeft ? nkLeft : nkRight);
            isLeft = !isLeft;

            yield return new WaitForSeconds(frameDuration);
            elapsed += frameDuration;
        }

        SetSprite(nkLeft);
        _wiggleCoroutine = null;
    }

    private IEnumerator IndefiniteWiggleCoroutine()
    {
        bool isLeft = true;
        const float frameDuration = 0.4f;

        while (true)
        {
            SetSprite(isLeft ? nkLeft : nkRight);
            isLeft = !isLeft;
            yield return new WaitForSeconds(frameDuration);
        }
    }


    private void OnDisable()
    {
        if (_wiggleCoroutine != null)
        {
            StopCoroutine(_wiggleCoroutine);
            _wiggleCoroutine = null;
        }
    }
}
