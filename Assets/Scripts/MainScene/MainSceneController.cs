using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainSceneController : MonoBehaviour
{

    [Header("Fade Screens")]
    [SerializeField] private GameObject fadeInScreen;
    [SerializeField] private GameObject fadeOutScreen;
    [SerializeField] private float fadeInDuration = 1f;
    [SerializeField] private float fadeOutDuration = 0.5f;

    [Header("Main UI Buttons")]
    [SerializeField] private Button tutorialButton;
    [SerializeField] private Button dailyCodeButton;
    [SerializeField] private Button inventoryButton;

    [Header("Popups")]
    [SerializeField] private PopupController tutorialPopup;
    [SerializeField] private PopupController dailyCodePopup;

    [Header("Tutorial Popup Buttons")]
    [SerializeField] private Button confirmTutorialButton;
    [SerializeField] private Button cancelTutorialButton;

    [Header("Daily Code Popup Buttons and Listeners")]
    [SerializeField] private Button cancelDailyCodeButton;
    [SerializeField] private DailyCodeInput dailyCodeInput;

    [Header("Sprite Elements")]
    [SerializeField] private SpriteRenderer draggableCoinRenderer;

    [Header("Video Reward")]
    [SerializeField] private Button watchVideoButton;
    [SerializeField] private VideoRewardController videoRewardController;

    private void Start()
    {
        StartCoroutine(StartMainScene());

        tutorialButton.onClick.AddListener(() =>
        {
            tutorialPopup.Open();
        });
        dailyCodeButton.onClick.AddListener(() =>
        {
            dailyCodePopup.Open();
        });
        inventoryButton.onClick.AddListener(() =>
        {
            GoToInventory();
        });

        if (watchVideoButton != null)
        {
            watchVideoButton.onClick.AddListener(OnWatchVideoPressed);
            UpdateVideoButtonVisibility();
        }

        if (videoRewardController != null)
            videoRewardController.OnVideoCompleted.AddListener(UpdateVideoButtonVisibility);

        confirmTutorialButton.onClick.AddListener(OnConfirmTutorial);
        cancelTutorialButton.onClick.AddListener(tutorialPopup.Close);


        cancelDailyCodeButton.onClick.AddListener(dailyCodePopup.Close);
        if (dailyCodeInput != null)
        {
            dailyCodeInput.OnCodeRedeemed.AddListener(OnDailyCodeSuccess);
        }

        UpdateDailyCodeButtonVisibility();

        if (CoinManager.Instance != null)
        {
            UpdateCoinVisibility(CoinManager.Instance.Coins);

            CoinManager.Instance.OnCoinsChanged.AddListener(UpdateCoinVisibility);
        }
        else
        {
            if (draggableCoinRenderer != null)
                draggableCoinRenderer.gameObject.SetActive(true);
        }
    }


    private void OnDestroy()
    {
        tutorialButton.onClick.RemoveAllListeners();
        dailyCodeButton.onClick.RemoveAllListeners();
        inventoryButton.onClick.RemoveAllListeners();
        confirmTutorialButton.onClick.RemoveAllListeners();
        cancelTutorialButton.onClick.RemoveAllListeners();
        cancelDailyCodeButton.onClick.RemoveAllListeners();
        if (dailyCodeInput != null)
        {
            dailyCodeInput.OnCodeRedeemed.RemoveListener(OnDailyCodeSuccess);
        }

        if (CoinManager.Instance != null)
        {
            CoinManager.Instance.OnCoinsChanged.RemoveListener(UpdateCoinVisibility);
        }

        if (watchVideoButton != null)
            watchVideoButton.onClick.RemoveAllListeners();

        if (videoRewardController != null)
            videoRewardController.OnVideoCompleted.RemoveListener(UpdateVideoButtonVisibility);
    }

    private void OnWatchVideoPressed()
    {
        if (videoRewardController != null)
            videoRewardController.StartVideo();
    }

    private void UpdateVideoButtonVisibility()
    {
        if (watchVideoButton != null && videoRewardController != null)
        {
            bool canWatch = videoRewardController.CanWatchVideo();
            watchVideoButton.gameObject.SetActive(canWatch);
        }
    }

    private IEnumerator StartMainScene()
    {
        fadeInScreen.SetActive(true);
        yield return new WaitForSeconds(fadeInDuration);
        fadeInScreen.SetActive(false);
    }

    private void OnConfirmTutorial()
    {
        tutorialPopup.Close();
        StartCoroutine(FadeOutAndLoadScene("TutorialScene"));
    }

    private void GoToInventory()
    {
        StartCoroutine(FadeOutAndLoadScene("InventoryScene"));
    }

    private IEnumerator FadeOutAndLoadScene(string sceneName)
    {
        fadeOutScreen.SetActive(true);
        yield return new WaitForSeconds(fadeOutDuration);
        SceneManager.LoadScene(sceneName);
    }

    private void OnDailyCodeSuccess()
    {
        dailyCodePopup.Close();
        UpdateDailyCodeButtonVisibility();
    }

    private void UpdateDailyCodeButtonVisibility()
    {
        if (CoinManager.Instance != null && dailyCodeButton != null)
        {
            bool alreadyRedeemed = CoinManager.Instance.HasRedeemedTodaysCode();
            dailyCodeButton.gameObject.SetActive(!alreadyRedeemed);
        }
    }

    private void UpdateCoinVisibility(int currentCoins)
    {
        if (draggableCoinRenderer == null) return;

        bool canAffordToPlay = currentCoins >= 100;

        draggableCoinRenderer.gameObject.SetActive(canAffordToPlay);
    }

    #region Debug Context Menu

    [ContextMenu("Debug/Reset Tutorial Status")]
    private void DebugResetTutorial()
    {
        TutorialSceneController.ResetTutorialStatus();
        Debug.Log("tutorial status reset. tutorial will load on next boot scene load.");
    }

    [ContextMenu("Debug/Reset All Player Data")]
    private void DebugResetAllData()
    {
        SecureStore.DeleteAll();
        Debug.Log("all player data cleared.");
    }

    [ContextMenu("Debug/Log Current Status")]
    private void DebugLogStatus()
    {
        bool tutorialDone = TutorialSceneController.HasCompletedTutorial();
        Debug.Log($"tutorial completed?: {tutorialDone}");

        if (CoinManager.Instance != null)
        {
            //Debug.Log($"Coins: {CoinManager.Instance.");
            Debug.Log($"today's code redeemed?: {CoinManager.Instance.HasRedeemedTodaysCode()}");
        }
    }

    [ContextMenu("Debug/Mark Tutorial Complete")]
    private void DebugMarkTutorialComplete()
    {
        TutorialSceneController.SetTutorialCompleted();
        Debug.Log("tutorial marked as complete.");
    }

    [ContextMenu("Debug/Display All User Data")]
    private void DebugDisplayAllUserData()
    {

        Debug.Log("--- COINS & LOGIN ---");
        int coins = SecureStore.GetInt("coins", 0);
        string lastLogin = SecureStore.GetString("lastLogin", "(none)");
        string redeemedDates = SecureStore.GetString("redeemedDates", "(none)");
        bool isNewPlayer = !SecureStore.HasKey("isNewPlayer");

        Debug.Log($"  Coins: {coins}");
        Debug.Log($"  Last Login: {lastLogin}");
        Debug.Log($"  Redeemed Code Dates: {redeemedDates}");
        Debug.Log($"  Is New Player: {isNewPlayer}");

        Debug.Log("--- TUTORIAL ---");
        Debug.Log($"  Tutorial Completed: {TutorialSceneController.HasCompletedTutorial()}");

        Debug.Log("--- COLLECTION ITEMS ---");
        string collectionPath = System.IO.Path.Combine(Application.persistentDataPath, "collection.json");

        if (System.IO.File.Exists(collectionPath))
        {
            try
            {
                string json = System.IO.File.ReadAllText(collectionPath);
                Debug.Log($"  Save file path: {collectionPath}");

                var save = JsonUtility.FromJson<CollectionSave>(json);

                if (save?.entries != null && save.entries.Count > 0)
                {
                    int totalOwned = 0;
                    int totalSeen = 0;

                    foreach (var entry in save.entries)
                    {
                        if (entry.amount > 0) totalOwned++;
                        if (entry.seen) totalSeen++;

                        Debug.Log($"  [{entry.id}] Amount: {entry.amount}, Seen: {entry.seen}");
                    }

                    Debug.Log($"  Collection summary: {totalOwned} owned, {totalSeen} seen (out of {save.entries.Count} entries in save)");
                }
                else
                {
                    Debug.Log("  No collection entries found in save file.");
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"  Failed to read collection file: {ex.Message}");
            }
        }
        else
        {
            Debug.Log($"  No collection save file exists at: {collectionPath}");
        }

        Debug.Log("=====================================");
    }

    [ContextMenu("Debug/Clear ALL User Data (Full Reset)")]
    private void DebugClearAllUserData()
    {
        SecureStore.DeleteAll();
        Debug.Log("Cleared SecureStore (PlayerPrefs) data.");

        string collectionPath = System.IO.Path.Combine(Application.persistentDataPath, "collection.json");
        if (System.IO.File.Exists(collectionPath))
        {
            System.IO.File.Delete(collectionPath);
            Debug.Log($"deleted: {collectionPath}");
        }

        string tempPath = collectionPath + ".tmp";
        if (System.IO.File.Exists(tempPath))
        {
            System.IO.File.Delete(tempPath);
            Debug.Log($"deleted: {tempPath}");
        }

        Debug.Log("ALL USER DATA CLEARED");
    }

    [ContextMenu("Debug/Add +1 of Every Collectible")]
    private void DebugAddOneOfEverything()
    {
        if (CollectionManager.Instance == null)
        {
            Debug.LogError("[Debug] CollectionManager Instance is null!");
            return;
        }

        int count = 0;
        foreach (var def in CollectionManager.Instance.Database.Items)
        {
            CollectionManager.Instance.AddItem(def.Id, 1, markSeen: true);
            count++;
        }

        Debug.Log($"[Debug] added +1 of {count} collectibles.");
    }

    #endregion
}