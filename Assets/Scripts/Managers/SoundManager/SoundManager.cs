using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;

    private AudioSource audioSource;

    [Header("SFX Clips")]
    public AudioClip buttonClick;
    public AudioClip explosion;
    public AudioClip pickup;
    public AudioClip turretShot;
    // Add more clips here as needed

    private void Awake()
    {
        // Singleton pattern
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject); // Optional: keep across scenes

        audioSource = GetComponent<AudioSource>();
    }

    // Play a specific clip
    public void PlaySFX(AudioClip clip)
    {
        if (clip != null)
            audioSource.PlayOneShot(clip);
        else
            Debug.LogWarning("SoundManager: AudioClip is null!");
    }

    // Convenience methods for specific sounds
    public void PlayButtonClick() => PlaySFX(buttonClick);
    public void PlayExplosion() => PlaySFX(explosion);
    public void PlayPickup() => PlaySFX(pickup);
    public void PlayShoot() => PlaySFX(turretShot);
}
