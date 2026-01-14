using System.Collections;
using UnityEngine;

[RequireComponent(typeof(TMPro.TMP_Text))]
public class DialogueCreator : MonoBehaviour
{
    public static TMPro.TMP_Text viewText;
    public static bool runTextPrint;
    public static int charCount;
    public static string transferText;

    void Awake()
    {
        viewText = GetComponent<TMPro.TMP_Text>();
        viewText.text = ""; 
    }

    void Update()
    {
        if (viewText == null) return;
        charCount = viewText.text.Length;

        if (runTextPrint)
        {
            runTextPrint = false;
            StopAllCoroutines();

            viewText.text = "";
            StartCoroutine(ScrollText());
        }
    }

    IEnumerator ScrollText()
    {
        if (string.IsNullOrEmpty(transferText))
            yield break;

        foreach (char c in transferText)
        {
            viewText.text += c;
            // charCount = viewText.text.Length;
            yield return new WaitForSeconds(0.03f);
        }
    }
}
