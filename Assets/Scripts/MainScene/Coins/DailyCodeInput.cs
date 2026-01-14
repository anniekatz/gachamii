using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class DailyCodeInput : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_InputField codeInputField;
    [SerializeField] private Button submitButton;
    [SerializeField] private TextMeshProUGUI feedbackText;

    [Header("Events")]
    public UnityEvent OnCodeRedeemed;

    private void Start()
    {
        submitButton.onClick.AddListener(OnSubmitCode);
    }

    private void OnEnable()
    {
        UpdateUI();

        if (codeInputField != null)
            codeInputField.text = "";
    }

    // button appears only if code has not been redeemed today, but this is just in case :D
    private void UpdateUI()
    {
        if (CoinManager.Instance == null) return;

        bool alreadyRedeemed = CoinManager.Instance.HasRedeemedTodaysCode();

        if (codeInputField != null)
            codeInputField.interactable = !alreadyRedeemed;

        if (submitButton != null)
            submitButton.interactable = !alreadyRedeemed;

        if (feedbackText != null)
        {
            feedbackText.text = alreadyRedeemed
                ? "Today's code already redeemed!"
                : "";
            feedbackText.color = Color.white;
        }
    }

    private void OnSubmitCode()
    {
        if (CoinManager.Instance == null || codeInputField == null) return;

        string code = codeInputField.text;
        bool success = CoinManager.Instance.TryRedeemDailyCode(code);

        if (feedbackText != null)
        {
            feedbackText.text = success
                ? ""
                : "Invalid code. Try again.";
            feedbackText.color = success ? Color.green : Color.red;
        }

        if (success)
        {
            codeInputField.text = "";
            UpdateUI();
            //notify listeners
            OnCodeRedeemed?.Invoke();
        }
    }

    private void OnDestroy()
    {
        submitButton.onClick.RemoveListener(OnSubmitCode);
    }
}