using System.Collections;
using UnityEngine;

public class KnobRotator : MonoBehaviour
{
    [Header("Rotation Settings")]
    [SerializeField] private float angleDelta = 90f;
    [SerializeField] private float duration = 3f;
    [SerializeField] private float returnDuration = 1.5f;
    [SerializeField] private bool playOnStart = false;

    private Coroutine _currentRoutine;

    private void Start()
    {
        if (playOnStart)
        {
            StartRotation();
        }
    }

    public void StartRotation()
    {
        if (_currentRoutine != null)
            StopCoroutine(_currentRoutine);

        _currentRoutine = StartCoroutine(RotateRoutine());
    }

    private IEnumerator RotateRoutine()
    {
        float elapsed = 0f;

        Quaternion startRot = transform.rotation;
        Quaternion endRot = startRot * Quaternion.Euler(0f, 0f, angleDelta);

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            transform.rotation = Quaternion.Slerp(startRot, endRot, t);

            yield return null;
        }

        transform.rotation = endRot;

        if (returnDuration > 0f)
        {
            elapsed = 0f;
            while (elapsed < returnDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / returnDuration);

                transform.rotation = Quaternion.Slerp(endRot, startRot, t);
                yield return null;
            }

            transform.rotation = startRot;
        }

        _currentRoutine = null;
    }
}