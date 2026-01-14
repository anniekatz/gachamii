using UnityEngine;
using System;
using System.Text;

public static class SecureStore
{
    private const string SECRET_KEY = "InsertSecretKeyHere";

    public static void SetInt(string key, int value)
    {
        string encrypted = Encrypt(value.ToString());
        string hash = ComputeHash(value.ToString());

        PlayerPrefs.SetString(key, encrypted);
        PlayerPrefs.SetString(key + "_h", hash);
        PlayerPrefs.Save();
    }

    public static int GetInt(string key, int defaultValue = 0)
    {
        string encrypted = PlayerPrefs.GetString(key, "");
        string storedHash = PlayerPrefs.GetString(key + "_h", "");

        if (string.IsNullOrEmpty(encrypted)) return defaultValue;

        try
        {
            string decrypted = Decrypt(encrypted);
            string computedHash = ComputeHash(decrypted);

            if (computedHash != storedHash)
            {
                Debug.LogWarning($"tampering detected 4 key: {key}");
                return defaultValue;
            }

            return int.Parse(decrypted);
        }
        catch
        {
            return defaultValue;
        }
    }

    public static void SetString(string key, string value)
    {
        string encrypted = Encrypt(value);
        string hash = ComputeHash(value);

        PlayerPrefs.SetString(key, encrypted);
        PlayerPrefs.SetString(key + "_h", hash);
        PlayerPrefs.Save();
    }

    public static string GetString(string key, string defaultValue = "")
    {
        string encrypted = PlayerPrefs.GetString(key, "");
        string storedHash = PlayerPrefs.GetString(key + "_h", "");

        if (string.IsNullOrEmpty(encrypted)) return defaultValue;

        try
        {
            string decrypted = Decrypt(encrypted);
            string computedHash = ComputeHash(decrypted);

            if (computedHash != storedHash)
            {
                Debug.LogWarning($"tampering detected 4 key: {key}");
                return defaultValue;
            }

            return decrypted;
        }
        catch
        {
            return defaultValue;
        }
    }

    public static bool HasKey(string key) => PlayerPrefs.HasKey(key);

    public static void DeleteKey(string key)
    {
        PlayerPrefs.DeleteKey(key);
        PlayerPrefs.DeleteKey(key + "_h");
    }

    public static void DeleteAll()
    {
        PlayerPrefs.DeleteAll();
    }

    private static string Encrypt(string plainText)
    {
        string xored = XorWithKey(plainText);
        return Convert.ToBase64String(Encoding.UTF8.GetBytes(xored));
    }

    private static string Decrypt(string encryptedText)
    {
        byte[] bytes = Convert.FromBase64String(encryptedText);
        string xored = Encoding.UTF8.GetString(bytes);
        return XorWithKey(xored);
    }

    private static string XorWithKey(string data)
    {
        StringBuilder result = new StringBuilder(data.Length);
        for (int i = 0; i < data.Length; i++)
        {
            result.Append((char)(data[i] ^ SECRET_KEY[i % SECRET_KEY.Length]));
        }
        return result.ToString();
    }

    private static string ComputeHash(string input)
    {
        string salted = input + SECRET_KEY;
        int hash = 0;
        foreach (char c in salted)
        {
            hash = (hash * 31 + c) & 0x7FFFFFFF;
        }
        return hash.ToString("X8");
    }

    public static void SetLong(string key, long value)
    {
        string encrypted = Encrypt(value.ToString());
        string hash = ComputeHash(value.ToString());

        PlayerPrefs.SetString(key, encrypted);
        PlayerPrefs.SetString(key + "_h", hash);
        PlayerPrefs.Save();
    }

    public static long GetLong(string key, long defaultValue = 0)
    {
        string encrypted = PlayerPrefs.GetString(key, "");
        string storedHash = PlayerPrefs.GetString(key + "_h", "");

        if (string.IsNullOrEmpty(encrypted)) return defaultValue;

        try
        {
            string decrypted = Decrypt(encrypted);
            string computedHash = ComputeHash(decrypted);

            if (computedHash != storedHash)
            {
                Debug.LogWarning($"tampering detected 4 key: {key}");
                return defaultValue;
            }

            return long.Parse(decrypted);
        }
        catch
        {
            return defaultValue;
        }
    }
}