using System.Collections;
using UnityEngine;


public class MachineShaker : MonoBehaviour
{
    [Header("Shake settings")]
    [Tooltip("How long the shake lasts (seconds).")]
    public float duration = 1f;

    [Tooltip("How far the object moves left/right.")]
    public float positionAmplitude = 0.025f;

    [Tooltip("How far the object tilts around Z (degrees).")]
    public float rotationAmplitude = 1f;

    [Tooltip("How fast the shake oscillates.")]
    public float frequency = 30f;

    [Tooltip("Optional falloff curve over time (0..1 on X, multiplier on Y).")]
    public AnimationCurve falloff = AnimationCurve.EaseInOut(0, 1, 1, 0);

    private Vector3 _originalLocalPos;
    private Quaternion _originalLocalRot;
    private Coroutine _shakeRoutine;

    private void Awake()
    {
        _originalLocalPos = transform.localPosition;
        _originalLocalRot = transform.localRotation;
    }

    public void TriggerShake()
    {
        if (!gameObject.activeInHierarchy)
            return;

        if (_shakeRoutine != null)
            StopCoroutine(_shakeRoutine);

        _shakeRoutine = StartCoroutine(ShakeCoroutine());
    }

    private IEnumerator ShakeCoroutine()
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float t = elapsed / duration;
            float strength = falloff.Evaluate(t);

            float offsetX = Mathf.Sin(elapsed * frequency) * positionAmplitude * strength;

            float rotZ = Mathf.Sin((elapsed + 0.1f) * frequency) * rotationAmplitude * strength;

            transform.localPosition = _originalLocalPos + new Vector3(offsetX, 0f, 0f);
            transform.localRotation = Quaternion.Euler(0f, 0f, rotZ) * _originalLocalRot;

            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.localPosition = _originalLocalPos;
        transform.localRotation = _originalLocalRot;
        _shakeRoutine = null;
    }
}
