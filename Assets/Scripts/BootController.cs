using UnityEngine;
using UnityEngine.SceneManagement;

public class BootController : MonoBehaviour
{
    private const string TUTORIAL_COMPLETED_KEY = "TutorialCompleted";

    private void Awake()
    {
        if (SecureStore.GetInt(TUTORIAL_COMPLETED_KEY, 0) == 1)
        {
            SceneManager.LoadScene("MainScene");
        }
        else
        {
            SceneManager.LoadScene("TutorialScene");
        }
    }
}