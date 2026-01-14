using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.Video;

public class VideoRewardController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private VideoPlayer videoPlayer;
    [SerializeField] private GameObject videoPanel;
    [SerializeField] private CanvasGroup videoPanelCanvasGroup;
    [SerializeField] private RawImage videoDisplay;
    [SerializeField] private Button closeButton;

    [Header("Settings")]
    [SerializeField] private int rewardAmount = 100;
    [SerializeField] private int cooldownDays = 3;
    [SerializeField] private float fadeDuration = 0.3f;

    [Header("Block Interactions")]
    [SerializeField] private CanvasGroup mainUICanvasGroup;

    [Header("Events")]
    public UnityEvent OnVideoCompleted;

    private const string LAST_WATCHED_KEY = "lastVideoWatchedDate";
    private bool _initialized;
    private bool _rewardPending;

    private void Initialize()
    {
        if (_initialized) return;
        _initialized = true;

        if (closeButton != null)
            closeButton.onClick.AddListener(CloseVideo);

        if (videoPlayer != null)
            videoPlayer.loopPointReached += OnVideoFinished;
    }

    private void OnDestroy()
    {
        if (closeButton != null)
            closeButton.onClick.RemoveListener(CloseVideo);

        if (videoPlayer != null)
            videoPlayer.loopPointReached -= OnVideoFinished;
    }

    public void StartVideo()
    {
        if (!CanWatchVideo()) return;

        _rewardPending = false;
        Initialize();
        StartCoroutine(StartVideoRoutine());
    }

    private IEnumerator StartVideoRoutine()
    {
        if (mainUICanvasGroup != null)
        {
            mainUICanvasGroup.interactable = false;
            mainUICanvasGroup.blocksRaycasts = false;
        }

        if (videoPanel != null)
            videoPanel.SetActive(true);

        if (closeButton != null)
            closeButton.gameObject.SetActive(false);

        if (videoPanelCanvasGroup != null)
        {
            videoPanelCanvasGroup.alpha = 0f;
            float elapsed = 0f;

            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                videoPanelCanvasGroup.alpha = Mathf.Clamp01(elapsed / fadeDuration);
                yield return null;
            }

            videoPanelCanvasGroup.alpha = 1f;
        }

        if (videoPlayer != null)
        {
            if (videoDisplay != null && videoPlayer.targetTexture != null)
                videoDisplay.texture = videoPlayer.targetTexture;

            videoPlayer.Play();
        }
    }

    public bool CanWatchVideo()
    {
        string lastWatchedStr = SecureStore.GetString(LAST_WATCHED_KEY, "");

        if (string.IsNullOrEmpty(lastWatchedStr))
            return true;

        if (DateTime.TryParse(lastWatchedStr, out DateTime lastWatched))
        {
            TimeSpan elapsed = DateTime.Now - lastWatched;
            return elapsed.TotalDays >= cooldownDays;
        }

        return true;
    }

    private void OnVideoFinished(VideoPlayer vp)
    {
        if (closeButton != null)
            closeButton.gameObject.SetActive(true);
        _rewardPending = true;


        SecureStore.SetString(LAST_WATCHED_KEY, DateTime.Now.ToString("o"));

        OnVideoCompleted?.Invoke();
    }

    private void CloseVideo()
    {
        StartCoroutine(CloseVideoRoutine());
    }

    private IEnumerator CloseVideoRoutine()
    {
        if (videoPlayer != null)
            videoPlayer.Stop();


        if (videoPanelCanvasGroup != null)
        {
            float elapsed = 0f;

            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                videoPanelCanvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);
                yield return null;
            }

            videoPanelCanvasGroup.alpha = 0f;
        }

        if (videoPanel != null)
            videoPanel.SetActive(false);


        if (mainUICanvasGroup != null)
        {
            mainUICanvasGroup.interactable = true;
            mainUICanvasGroup.blocksRaycasts = true;
        }

        if (_rewardPending)
        {
            _rewardPending = false;

            if (CoinManager.Instance != null)
                CoinManager.Instance.AddCoins(rewardAmount);
        }
    }

    [ContextMenu("Debug/Reset Video Cooldown")]
    private void DebugResetCooldown()
    {
        SecureStore.DeleteKey(LAST_WATCHED_KEY);
        Debug.Log("[VideoReward] cooldown reset.");
    }
}