using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallJumblerOld : MonoBehaviour
{
    [Header("Jumble settings")]
    public float shakeDuration = 2f;
    public float shakeForce = 5f;
    public float kickInterval = 0.1f;
    public float maxSpeed = 10f;

    [Header("Movement window")]
    public float activeDuration = 5f;

    [Header("Ball discovery")]
    public string ballTag = "Ball";

    private readonly List<Rigidbody2D> _balls = new List<Rigidbody2D>();
    private Coroutine _currentJumbleRoutine;
    private Coroutine _freezeTimerRoutine;

    private void Awake()
    {
        RefreshBalls();
        Debug.Log("[BallJumbler] Found " + _balls.Count + " balls in Awake");
    }

    private void Start()
    {
        StartMovementWindow();
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

    public void Jumble()
    {
        Debug.Log("[BallJumbler] Jumble() called");

        RefreshBalls();
        StartMovementWindow();

        if (_currentJumbleRoutine != null)
        {
            StopCoroutine(_currentJumbleRoutine);
        }

        _currentJumbleRoutine = StartCoroutine(JumbleRoutine());
    }


    private void StartMovementWindow()
    { 
        UnfreezeBalls();


        if (_freezeTimerRoutine != null)
        {
            StopCoroutine(_freezeTimerRoutine);
        }
        _freezeTimerRoutine = StartCoroutine(FreezeAfterDelay(activeDuration));
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

        Debug.Log("[BallJumbler] Balls frozen");
    }

    private void UnfreezeBalls()
    {
        foreach (var rb in _balls)
        {
            if (rb == null) continue;

            rb.simulated = true;
        }

        Debug.Log("[BallJumbler] Balls unfrozen");
    }



    private IEnumerator JumbleRoutine()
    {
        Debug.Log("[BallJumbler] Starting JumbleRoutine with " + _balls.Count + " balls");

        float endTime = Time.time + shakeDuration;

        while (Time.time < endTime)
        {
            foreach (var rb in _balls)
            {
                if (rb == null || !rb.simulated) continue;

                Vector2 dir = Random.insideUnitCircle.normalized;
                float mag = Random.Range(0.5f * shakeForce, 1.0f * shakeForce);

                rb.AddForce(dir * mag, ForceMode2D.Impulse);

                if (rb.linearVelocity.magnitude > maxSpeed)
                {
                    rb.linearVelocity = rb.linearVelocity.normalized * maxSpeed;
                }
            }

            yield return new WaitForSeconds(kickInterval);
        }

        Debug.Log("[BallJumbler] JumbleRoutine finished");
        _currentJumbleRoutine = null;
    }
}
