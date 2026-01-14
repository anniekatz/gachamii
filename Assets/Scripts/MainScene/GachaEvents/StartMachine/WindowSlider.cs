using System.Collections;
using UnityEngine;

public class WindowSlider : MonoBehaviour
{
    [SerializeField] private float slideDistance = 2f;
    [SerializeField] private float duration = 1.5f;

    private Vector3 _startLocalPos;
    private Coroutine _routine;

    private void Awake()
    {
        _startLocalPos = transform.localPosition;
    }

    public void SlideUp()
    {
        if (_routine != null) StopCoroutine(_routine);
        Vector3 endPos = _startLocalPos + Vector3.up * slideDistance;
        _routine = StartCoroutine(SlideRoutine(transform.localPosition, endPos));
    }

    public void SlideDown()
    {
        if (_routine != null) StopCoroutine(_routine);
        _routine = StartCoroutine(SlideRoutine(transform.localPosition, _startLocalPos));
    }

    public void ResetImmediate()
    {
        if (_routine != null)
        {
            StopCoroutine(_routine);
            _routine = null;
        }
        transform.localPosition = _startLocalPos;
    }


    private IEnumerator SlideRoutine(Vector3 from, Vector3 to)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            transform.localPosition = Vector3.Lerp(from, to, t);
            yield return null;
        }
        transform.localPosition = to;
        _routine = null;
    }
}
