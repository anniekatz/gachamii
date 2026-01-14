using System.Collections;
using UnityEngine;

public class CapsuleDropper : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform capsuleTransform;
    [SerializeField] private Transform startPoint;
    [SerializeField] private Transform endPoint;
    [SerializeField] private SpriteRenderer capsuleRenderer;

    [Header("Timing")]
    [SerializeField] private float dropDuration = 0.5f;

    [Header("Bounce")]
    [SerializeField] private float bounceHeight = 0.1f;
    [SerializeField] private float bounceDuration = 0.2f;
    [SerializeField] private int bounceCount = 1;

    [SerializeField] private MakeDroppedCapsuleClickable capsuleTap;

    private Coroutine _currentRoutine;


    private void Awake()
    {
        if (capsuleTransform == null)
            capsuleTransform = transform.Find("DroppedCapsule");

        if (startPoint == null)
            startPoint = transform.Find("StartPoint");

        if (endPoint == null)
            endPoint = transform.Find("EndPoint");

        if (capsuleRenderer == null && capsuleTransform != null)
            capsuleRenderer = capsuleTransform.GetComponent<SpriteRenderer>();

        if (capsuleTransform != null && capsuleTap == null)
            capsuleTap = capsuleTransform.GetComponent<MakeDroppedCapsuleClickable>();

        Debug.Log($"[CapsuleDropper] capsuleTap is {(capsuleTap ? "SET" : "NULL")}");
    }

    private void Reset()
    {
        if (capsuleTransform == null)
            capsuleTransform = transform.Find("DroppedCapsule");

        if (startPoint == null)
            startPoint = transform.Find("StartPoint");

        if (endPoint == null)
            endPoint = transform.Find("EndPoint");

        if (capsuleRenderer == null && capsuleTransform != null)
            capsuleRenderer = capsuleTransform.GetComponent<SpriteRenderer>();

        if (capsuleTap == null && capsuleTransform != null)
            capsuleTap = capsuleTransform.GetComponent<MakeDroppedCapsuleClickable>();
    }

    public void PlayDrop(Sprite capsuleSprite = null)
    {
        if (capsuleSprite != null && capsuleRenderer != null)
        {
            capsuleRenderer.sprite = capsuleSprite;
        }

        if (_currentRoutine != null)
            StopCoroutine(_currentRoutine);

        _currentRoutine = StartCoroutine(DropRoutine());
    }

    private IEnumerator DropRoutine()
    {
        capsuleTransform.position = startPoint.position;
        capsuleTransform.gameObject.SetActive(true);

        Vector3 startPos = startPoint.position;
        Vector3 endPos = endPoint.position;

        float elapsed = 0f;
        while (elapsed < dropDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / dropDuration);

            float eased = t * t;

            capsuleTransform.position = Vector3.Lerp(startPos, endPos, eased);

            yield return null;
        }

        capsuleTransform.position = endPos;

        if (bounceHeight > 0f && bounceDuration > 0f && bounceCount > 0)
        {
            for (int i = 0; i < bounceCount; i++)
            {
                float amp = bounceHeight / (i + 1);
                float bounceElapsed = 0f;

                while (bounceElapsed < bounceDuration)
                {
                    bounceElapsed += Time.deltaTime;
                    float t = Mathf.Clamp01(bounceElapsed / bounceDuration);

                    float offsetY = Mathf.Sin(t * Mathf.PI) * amp;

                    capsuleTransform.position = endPos + Vector3.up * offsetY;

                    yield return null;
                }

                capsuleTransform.position = endPos;
            }
        }

        _currentRoutine = null;

        capsuleTap?.SetArmed(true);
    }
}
