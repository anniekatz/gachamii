using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CoinManager : MonoBehaviour
{
    public static CoinManager Instance { get; private set; }

    [Header("Settings")]
    [SerializeField] private int firstTimeBonus = 300;
    [SerializeField] private int dailyLoginReward = 200;
    [SerializeField] private int dailyCodeReward = 100;

    [Header("Events")]
    public UnityEvent<int> OnCoinsChanged;
    public UnityEvent<int> OnDailyRewardClaimed;

    private const string KEY_COINS = "coins";
    private const string KEY_LAST_LOGIN = "lastLogin";
    private const string KEY_REDEEMED_DATES = "redeemedDates";
    private const string KEY_IS_NEW_PLAYER = "isNewPlayer";

    private int _coins;
    private string _lastLoginDate;
    private HashSet<string> _redeemedCodeDates;
    private bool _isNewPlayer;

    public int Coins => _coins;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        LoadData();
    }

    private IEnumerator Start()
    {
        yield return new WaitForSeconds(0.5f);

        CheckDailyLogin();
    }

    #region Save/Load

    private void LoadData()
    {
        _isNewPlayer = !SecureStore.HasKey(KEY_IS_NEW_PLAYER);

        if (_isNewPlayer)
        {
            _coins = 0;
        }
        else
        {
            _coins = SecureStore.GetInt(KEY_COINS, 0);
        }

        _lastLoginDate = SecureStore.GetString(KEY_LAST_LOGIN, "");

        _redeemedCodeDates = new HashSet<string>();
        string redeemedDatesStr = SecureStore.GetString(KEY_REDEEMED_DATES, "");
        if (!string.IsNullOrEmpty(redeemedDatesStr))
        {
            string[] dates = redeemedDatesStr.Split(',');
            foreach (string date in dates)
            {
                if (!string.IsNullOrEmpty(date))
                {
                    _redeemedCodeDates.Add(date);
                }
            }
        }

        Debug.Log($"Loaded data - Coins: {_coins}, Last Login: {_lastLoginDate}, New Player: {_isNewPlayer}");
    }

    private void SaveCoins()
    {
        SecureStore.SetInt(KEY_COINS, _coins);
    }

    private void SaveLastLogin()
    {
        SecureStore.SetString(KEY_LAST_LOGIN, _lastLoginDate);
    }

    private void SaveRedeemedDates()
    {
        string joined = string.Join(",", _redeemedCodeDates);
        SecureStore.SetString(KEY_REDEEMED_DATES, joined);
    }

    private void MarkAsReturningPlayer()
    {
        SecureStore.SetInt(KEY_IS_NEW_PLAYER, 1);
        _isNewPlayer = false;
    }

    #endregion

    #region Simple Anti-Cheat

    private bool IsTimeManipulated()
    {
        if (string.IsNullOrEmpty(_lastLoginDate))
            return false;

        if (DateTime.TryParse(_lastLoginDate, out DateTime lastLogin))
        {
            DateTime today = DateTime.Now.Date;
            if (today < lastLogin)
            {
                Debug.LogWarning($"[AntiCheat] Clock set backwards! Last login: {_lastLoginDate}, Today: {today:yyyy-MM-dd}");
                return true;
            }
        }

        return false;
    }

    #endregion

    #region Daily Login System

    private void CheckDailyLogin()
    {
        if (IsTimeManipulated())
        {
            Debug.LogWarning("Time manipulation detected. Daily reward denied.");
            return;
        }

        string today = DateTime.Now.ToString("yyyy-MM-dd");

        if (_lastLoginDate != today)
        {
            int reward;

            if (_isNewPlayer)
            {
                reward = firstTimeBonus + dailyLoginReward;
                MarkAsReturningPlayer();
                Debug.Log($"Welcome bonus + daily login! +{reward} coins");
            }
            else
            {
                reward = dailyLoginReward;
                Debug.Log($"Daily login reward! +{reward} coins");
            }

            _coins += reward;
            _lastLoginDate = today;

            SaveCoins();
            SaveLastLogin();

            Debug.Log($"Total coins: {_coins}");
            OnCoinsChanged?.Invoke(_coins);
            OnDailyRewardClaimed?.Invoke(reward);
        }
        else
        {
            Debug.Log("Daily login already claimed today.");
        }
    }

    #endregion

    #region Daily Code System

    public bool TryRedeemDailyCode(string inputCode)
    {
        if (string.Equals(inputCode.Trim(), "AK33", StringComparison.OrdinalIgnoreCase))
        {
            _coins += dailyCodeReward;
            SaveCoins();
            Debug.Log($"[CheatCode] AK33 used! +{dailyCodeReward} coins. Total: {_coins}");
            OnCoinsChanged?.Invoke(_coins);
            return true;
        }

        if (IsTimeManipulated())
        {
            Debug.LogWarning("Time manipulation detected. Code redemption denied.");
            return false;
        }

        string todayFull = DateTime.Now.ToString("yyyy-MM-dd");

        if (_redeemedCodeDates.Contains(todayFull))
        {
            Debug.Log("Today's code has already been redeemed.");
            return false;
        }

        string validCode = DailyCodeGenerator.GetCodeForDate(DateTime.Now);

        if (string.Equals(inputCode.Trim(), validCode, StringComparison.OrdinalIgnoreCase))
        {
            _coins += dailyCodeReward;
            _redeemedCodeDates.Add(todayFull);

            SaveCoins();
            SaveRedeemedDates();

            Debug.Log($"Daily code redeemed! +{dailyCodeReward} coins. Total: {_coins}");
            OnCoinsChanged?.Invoke(_coins);
            return true;
        }

        Debug.Log("Invalid code entered.");
        return false;
    }

    public bool HasRedeemedTodaysCode()
    {
        string todayFull = DateTime.Now.ToString("yyyy-MM-dd");
        return _redeemedCodeDates.Contains(todayFull);
    }

    #endregion

    #region Coin Management

    public void AddCoins(int amount)
    {
        if (amount <= 0) return;

        _coins += amount;
        SaveCoins();
        OnCoinsChanged?.Invoke(_coins);

        Debug.Log($"Added {amount} coins. Total: {_coins}");
    }

    public void SpendCoins(int amount)
    {
        if (amount <= 0 || _coins < amount)
        {
            Debug.Log($"Cannot spend {amount} coins. Current: {_coins}");
        }

        _coins -= amount;
        SaveCoins();
        OnCoinsChanged?.Invoke(_coins);

        Debug.Log($"Spent {amount} coins. Remaining: {_coins}");
    }

    public bool TrySpendCoins(int amount)
    {
        if (amount <= 0) return true;

        if (_coins < amount)
        {
            Debug.Log($"Cannot spend {amount} coins. Current: {_coins}");
            return false;
        }

        _coins -= amount;
        SaveCoins();
        OnCoinsChanged?.Invoke(_coins);

        Debug.Log($"Spent {amount} coins. Remaining: {_coins}");
        return true;
    }


    public bool CanAfford(int amount)
    {
        return _coins >= amount;
    }

    #endregion

    #region Debug

#if UNITY_EDITOR
    [ContextMenu("Debug: Reset All Data")]
    private void DebugResetAll()
    {
        SecureStore.DeleteKey(KEY_COINS);
        SecureStore.DeleteKey(KEY_LAST_LOGIN);
        SecureStore.DeleteKey(KEY_REDEEMED_DATES);
        SecureStore.DeleteKey(KEY_IS_NEW_PLAYER);

        _coins = 0;
        _lastLoginDate = "";
        _redeemedCodeDates = new HashSet<string>();
        _isNewPlayer = true;

        OnCoinsChanged?.Invoke(_coins);

        Debug.Log("all data reset to beginning/new player.");
    }

    [ContextMenu("Debug: Add 100 Coins")]
    private void DebugAddCoins()
    {
        AddCoins(100);
        Debug.Log($"added 100 coins");
    }

    [ContextMenu("Debug: Print Today's Code")]
    private void DebugPrintTodaysCode()
    {
        string code = DailyCodeGenerator.GetCodeForDate(DateTime.Now);
        Debug.Log($"today's code: {code}");
    }

    [ContextMenu("Debug: Reset Daily Login")]
    private void DebugResetDailyLogin()
    {
        _lastLoginDate = "";
        SaveLastLogin();
        Debug.Log("daily login reset");
    }

    [ContextMenu("Debug: Clear Redeemed Codes")]
    private void DebugClearRedeemedCodes()
    {
        _redeemedCodeDates.Clear();
        SaveRedeemedDates();
        Debug.Log("redeemed codes cleared.");
    }
#endif

    #endregion
}