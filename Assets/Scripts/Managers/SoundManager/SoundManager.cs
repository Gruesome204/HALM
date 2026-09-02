using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    private AudioSource audioSource;

    [Header("SFX Clips")]
    public AudioClip paperButtonClick;
    public AudioClip woodButtonClick;
    public AudioClip stoneButtonClick;

    public AudioClip paperMenuOpen;
    public AudioClip woodMenuOpen;
    public AudioClip stoneMenuOpen;

    public AudioClip towerShootSound;
    public AudioClip towerBuildSound;
    public AudioClip towerDestroySound;

    public AudioClip roomClearedSound;

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

    private void Start()
    {
        EnemySpawnManager.Instance.OnAllEnemiesDefeated += PlayRoomCleared;
    }
    private void OnDisable()
    {
        EnemySpawnManager.Instance.OnAllEnemiesDefeated -= PlayRoomCleared;
    }
    // Play a specific clip
    public void PlaySFX(AudioClip clip)
    {
        if (clip != null)
        {
            audioSource.volume = GameManager.Instance.gameDataSO.masterVolume * GameManager.Instance.gameDataSO.soundVolume;
            audioSource.PlayOneShot(clip);
        }
        else
        {
            Debug.LogWarning("SoundManager: AudioClip is null!");
        }
    }

    // Convenience methods for specific sounds
    //Menu Sounds
    public void PlayPaperClick() => PlaySFX(paperButtonClick);
    public void PlayWoodClick() => PlaySFX(woodButtonClick);
    public void PlayStoneClick() => PlaySFX(stoneButtonClick);

    public void PlayPaperMenuOpen() => PlaySFX(paperMenuOpen);
    public void PlayWoodMenuOpen() => PlaySFX(woodMenuOpen);
    public void PlayStoneMenuOpen() => PlaySFX(stoneMenuOpen);


    //Tower Sounds
    public void PlayTowerShoot() => PlaySFX(towerShootSound);
    public void PlayTowerBuild() => PlaySFX(towerBuildSound);
    public void PlayTowerDestroy() => PlaySFX(towerDestroySound);

    //Enemies Defeated
    public void PlayRoomCleared() => PlaySFX(roomClearedSound);
}
