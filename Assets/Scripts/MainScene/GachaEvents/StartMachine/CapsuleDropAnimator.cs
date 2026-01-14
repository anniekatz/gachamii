using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CapsuleDropAnimator : MonoBehaviour
{
    //animate knob rotate, ball jumble, and capsule drop
    [Header("Knob link")]
    [SerializeField] private KnobRotator knobRotator;
    [Tooltip("Default delay between knob start and ball jumble start: 1s (knob default turn is 3 sec)")]
    [SerializeField] private float delayBeforeJumble = 1f;

    [Header("Window Slider link")]
    [SerializeField] private WindowSlider windowSlider;
    [Tooltip("Delay between ball jumble and window slider")]
    [SerializeField] private float delayBeforeSlider = 3f;

    [Header("Capsule Dropper link")]
    [SerializeField] private CapsuleDropper capsuleDropper;
    [Tooltip("Delay between window slider and ball drop")]
    [SerializeField] private float delayBeforeDrop = 1f;

    [Header("Coin to animate")]
    [Tooltip("The coin GameObject to animate during the drop.")]
    [SerializeField] public GameObject coinToAnimate;

    [Header("Jumble settings")]
    [Tooltip("Max horizontal speed applied on jumble.")]
    public float horizontalSpeed = 8f;

    [Tooltip("Min vertical speed (upwards) applied on jumble.")]
    public float minVerticalSpeed = 6f;

    [Tooltip("Max vertical speed (upwards) applied on jumble.")]
    public float maxVerticalSpeed = 12f;

    [Tooltip("Max absolute angular velocity (degrees/sec).")]
    public float maxAngularSpeed = 360f;

    [Tooltip("Clamp linear speed after jumble.")]
    public float maxSpeed = 20f;

    [Header("Movement window after start")]
    [Tooltip("How long balls are allowed to move before being frozen again.")]
    public float activeStartDuration = 3f;

    [Header("Movement window after jumble")]
    [Tooltip("How long balls are allowed to move before being frozen again.")]
    public float activeDuration = 5f;


    [Header("Ball discovery")]
    public string ballTag = "Ball";

    [Header("MachineShaker link")]
    [SerializeField] private MachineShaker shaker;

    private readonly List<Rigidbody2D> _balls = new List<Rigidbody2D>();

    private Coroutine _freezeTimerRoutine;
    private Coroutine _sequenceRoutine;

    private void Awake()
    {
        RefreshBalls();
        if (coinToAnimate != null)
        {
            coinToAnimate.SetActive(false);
        }
    }

    private void Start()
    {
        StartMovementWindow(activeStartDuration);
    }

    public void StartCapsuleDropAnimator()
    {
        if (_sequenceRoutine != null)
            StopCoroutine(_sequenceRoutine);
        _sequenceRoutine = StartCoroutine(RunSequence());
    }

    private IEnumerator RunSequence()
    {
        if (coinToAnimate != null)
        {
            coinToAnimate.SetActive(true);
            yield return new WaitForSeconds(1f);
            coinToAnimate.SetActive(false);
        }

        if (shaker != null)
        {

            Debug.Log("shaker is not null");
            shaker.TriggerShake();
        }
        else
        {
            Debug.Log("shaker is null");
        }

        yield return new WaitForSeconds(0.2f);


        if (knobRotator != null)
        {
            knobRotator.StartRotation();
        }


        yield return new WaitForSeconds(delayBeforeJumble);


        StartCoroutine(JumbleRoutine());

        yield return new WaitForSeconds(delayBeforeSlider);

        if (windowSlider != null)
        {
            windowSlider.SlideUp();
        }

        yield return new WaitForSeconds(delayBeforeDrop);

        if (capsuleDropper != null)
        {
            capsuleDropper.PlayDrop();
        }
    }

    private void RefreshBalls()
    {
        _balls.Clear();

        var ballObjects = GameObject.FindGameObjectsWithTag(ballTag);
        foreach (var go in ballObjects)
        {
            var rb = go.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                _balls.Add(rb);
            }
        }
    }

    private IEnumerator JumbleRoutine()
    {

        RefreshBalls();
        StartMovementWindow(activeDuration);


        int kickCount = UnityEngine.Random.Range(1, 4);

        float totalWindow = 0.75f;


        float delayBetweenKicks = (kickCount > 1)
            ? totalWindow / (kickCount - 1)
            : 0f;

        for (int i = 0; i < kickCount; i++)
        {
            KickBallsLmao();


            if (i < kickCount - 1 && delayBetweenKicks > 0f)
            {
                yield return new WaitForSeconds(delayBetweenKicks);
            }
        }

    }

    private void KickBallsLmao()
    {

        foreach (var rb in _balls)
        {
            if (rb == null) continue;

            rb.simulated = true;
            rb.WakeUp();

            float vx = UnityEngine.Random.Range(-horizontalSpeed, horizontalSpeed);


            float vy = UnityEngine.Random.Range(minVerticalSpeed, maxVerticalSpeed);

            Vector2 v = new Vector2(vx, vy);


            if (v.magnitude > maxSpeed)
            {
                v = v.normalized * maxSpeed;
            }

            rb.linearVelocity = v;


            float angVel = UnityEngine.Random.Range(-maxAngularSpeed, maxAngularSpeed);
            rb.angularVelocity = angVel;
        }
    }

    private void StartMovementWindow(float duration)
    {
        UnfreezeBalls();

        if (_freezeTimerRoutine != null)
        {
            StopCoroutine(_freezeTimerRoutine);
        }
        _freezeTimerRoutine = StartCoroutine(FreezeAfterDelay(duration));
    }

    private IEnumerator FreezeAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        FreezeBalls();
        _freezeTimerRoutine = null;
    }

    private void FreezeBalls()
    {
        foreach (var rb in _balls)
        {
            if (rb == null) continue;

            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.simulated = false;
        }

    }

    private void UnfreezeBalls()
    {
        foreach (var rb in _balls)
        {
            if (rb == null) continue;

            rb.simulated = true;
        }
    }
}
