using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public sealed class PersistBackgroundMusic : MonoBehaviour
{
    public static PersistBackgroundMusic Instance { get; private set; }

    private AudioSource audioSource;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        audioSource = GetComponent<AudioSource>();
        audioSource.loop = true;

        if (!audioSource.isPlaying && audioSource.clip != null)
            audioSource.Play();
    }
}
