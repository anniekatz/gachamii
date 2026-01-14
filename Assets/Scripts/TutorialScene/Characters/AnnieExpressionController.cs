using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public sealed class AnnieExpressionController : MonoBehaviour
{
    [Header("Expression Sprites")]
    public Sprite akDefault;
    public Sprite akHappy;
    public Sprite akClap;
    public Sprite akClosed;
    public Sprite akDetermined;
    public Sprite akExplain;
    public Sprite akPoint;

    private Image _image;

    private void Awake()
    {
        _image = GetComponent<Image>();
        _image.preserveAspect = true;

        if (akDefault != null)
            _image.sprite = akDefault;
    }

    private void SetSprite(Sprite s)
    {
        if (s == null || _image == null) return;
        _image.sprite = s;
    }

    public void SetAnnieDefault() => SetSprite(akDefault);
    public void SetAnnieHappy() => SetSprite(akHappy);
    public void SetAnnieClap() => SetSprite(akClap);
    public void SetAnnieClosed() => SetSprite(akClosed);
    public void SetAnnieDetermined() => SetSprite(akDetermined);
    public void SetAnnieExplain() => SetSprite(akExplain);
    public void SetAnniePoint() => SetSprite(akPoint);
}
