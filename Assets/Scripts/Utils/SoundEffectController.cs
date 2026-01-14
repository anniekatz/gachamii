using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public sealed class SoundEffectController : MonoBehaviour
{
    public static SoundEffectController Instance { get; private set; }

    [SerializeField] private AudioSource soundSource;

    [System.Serializable]
    public class NamedClip
    {
        public string key;
        public AudioClip clip;
    }

    [SerializeField] private NamedClip[] soundClips;

    private Dictionary<string, AudioClip> soundLookup;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (soundSource == null)
            soundSource = GetComponent<AudioSource>();

        soundLookup = new Dictionary<string, AudioClip>();
        foreach (var nc in soundClips)
        {
            if (!string.IsNullOrEmpty(nc.key) && nc.clip != null)
                soundLookup[nc.key] = nc.clip;
        }
    }

    public void PlaySound(string key)
    {
        if (soundSource == null || soundLookup == null) return;

        if (soundLookup.TryGetValue(key, out var clip) && clip != null)
        {
            soundSource.PlayOneShot(clip);
        }
        else
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.LogWarning($"[SoundEffectController] No sound clip for key '{key}'");
#endif
        }
    }
}
