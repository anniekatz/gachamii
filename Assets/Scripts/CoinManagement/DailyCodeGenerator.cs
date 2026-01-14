using System;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

// generate unique code based on date
public static class DailyCodeGenerator
{
    private const string SecretSeed = "InsertSecretSeedHere";
    public static string GetCodeForDate(DateTime date)
    {
        string dateKey = date.ToString("yyyy-MM-dd");
        string input = $"{SecretSeed}-{dateKey}";

        using (SHA256 sha256 = SHA256.Create())
        {
            byte[] hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));

            StringBuilder code = new StringBuilder();
            for (int i = 0; i < 4; i++)
            {
                int value = hashBytes[i] % 36;
                if (value < 10)
                    code.Append((char)('0' + value));
                else
                    code.Append((char)('A' + value - 10));
            }

            return code.ToString();
        }
    }

    public static string GenerateCodesForYear(int year)
    {
        StringBuilder sb = new StringBuilder();
        DateTime startDate = new DateTime(year, 1, 1);
        DateTime endDate = new DateTime(year, 12, 31);

        sb.AppendLine($"=== Daily Codes for {year} ===");
        sb.AppendLine();

        DateTime current = startDate;
        while (current <= endDate)
        {
            string code = GetCodeForDate(current);
            sb.AppendLine($"{current:yyyy-MM-dd}: {code}");
            current = current.AddDays(1);
        }

        return sb.ToString();
    }

    public static string GenerateCodesForYearRange(int startYear, int endYear)
    {
        StringBuilder sb = new StringBuilder();

        for (int year = startYear; year <= endYear; year++)
        {
            sb.AppendLine(GenerateCodesForYear(year));
            sb.AppendLine();
        }

        return sb.ToString();
    }

    public static string GenerateCodesForDateRange(DateTime startDate, DateTime endDate)
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine($"=== Daily Codes: {startDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd} ===");
        sb.AppendLine();

        DateTime current = startDate;
        while (current <= endDate)
        {
            string code = GetCodeForDate(current);
            sb.AppendLine($"{current:yyyy-MM-dd}: {code}");
            current = current.AddDays(1);
        }

        return sb.ToString();
    }

    public static string GenerateCodesAsCsv(int startYear, int endYear)
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("Date,Code,DayOfWeek,Year,Month,Day");

        for (int year = startYear; year <= endYear; year++)
        {
            DateTime startDate = new DateTime(year, 1, 1);
            DateTime endDate = new DateTime(year, 12, 31);

            DateTime current = startDate;
            while (current <= endDate)
            {
                string code = GetCodeForDate(current);
                sb.AppendLine($"{current:yyyy-MM-dd},{code},{current.DayOfWeek},{current.Year},{current.Month},{current.Day}");
                current = current.AddDays(1);
            }
        }

        return sb.ToString();
    }

#if UNITY_EDITOR
    [UnityEditor.MenuItem("Tools/Daily Codes/Print This Year's Codes")]
    public static void PrintThisYearCodes()
    {
        string codes = GenerateCodesForYear(DateTime.Now.Year);
        Debug.Log(codes);
    }

    [UnityEditor.MenuItem("Tools/Daily Codes/Print Today's Code")]
    public static void PrintTodaysCode()
    {
        string code = GetCodeForDate(DateTime.Now);
        Debug.Log($"today's code: {code}");
    }

    [UnityEditor.MenuItem("Tools/Daily Codes/Export 5 Years as CSV")]
    public static void ExportTenYearsAsCsv()
    {
        int currentYear = DateTime.Now.Year;
        string csv = GenerateCodesAsCsv(currentYear, currentYear + 4);

        string path = System.IO.Path.Combine(Application.dataPath, "../DailyCodes.csv");
        System.IO.File.WriteAllText(path, csv);

        Debug.Log($"exported 5 years of codes");
        UnityEditor.EditorUtility.RevealInFinder(path);
    }

    [UnityEditor.MenuItem("Tools/Daily Codes/Export Custom Range")]
    public static void ExportCustomRange()
    {
        int startYear = 2025;
        int endYear = 2030;

        string csv = GenerateCodesAsCsv(startYear, endYear);
        string path = System.IO.Path.Combine(Application.dataPath, $"../DailyCodes_{startYear}-{endYear}.csv");
        System.IO.File.WriteAllText(path, csv);

        Debug.Log($"exported codes from {startYear} to {endYear}");
        UnityEditor.EditorUtility.RevealInFinder(path);
    }
#endif
}