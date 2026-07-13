using UnityEngine;

public class AudioManager : MonoBehaviour
{
    // --- ПАТТЕРН СИНГЛТОН ---
    public static AudioManager Instance { get; private set; }

    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("Фоновая музыка (BGM)")]
    [SerializeField] private AudioClip backgroundMusic;
    [SerializeField] private AudioClip mainMenuMusic;

    [Header("Звуки Игрока")]
    [SerializeField] private AudioClip playerFootstep;
    [SerializeField] private AudioClip playerDash;
    [SerializeField] private AudioClip playerAttack;
    [SerializeField] private AudioClip playerTakeDamage;

    [Header("Звуки Боссов (Удары)")]
    [SerializeField] private AudioClip boss1Hit;
    [SerializeField] private AudioClip boss2Hit;
    [SerializeField] private AudioClip boss3Hit;

    [Header("Окружение и UI")]
    [SerializeField] private AudioClip corridorAppear;
    [SerializeField] private AudioClip introRevealSound; // Добавлено из вашего примера
    [SerializeField] private AudioClip buttonClick;

    [Header("Звуки анимации букв")]
    [SerializeField] private AudioClip letterMoveStandard;
    [SerializeField] private AudioClip letterMoveFast;
    [SerializeField] private AudioClip letterMoveImpact;
    [SerializeField] private AudioClip[] randomLetterSounds;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // ==========================================
    // ВНУТРЕННИЕ МЕТОДЫ ВОСПРОИЗВЕДЕНИЯ (Private)
    // ==========================================

    private void PlayMusic(AudioClip clip)
    {
        if (clip == null) return;
        musicSource.clip = clip;
        musicSource.loop = true;
        musicSource.Play();
    }

    private void PlaySFX(AudioClip clip)
    {
        if (clip == null) return;
        sfxSource.PlayOneShot(clip);
    }

    // ==========================================
    // БЕЗОПАСНЫЕ СТАТИЧНЫЕ МЕТОДЫ (Safe Call)
    // ==========================================
    // Именно по такому принципу, как вы просили. 
    // Теперь они доступны из любой точки кода.

    public static void PlayBackgroundMusic()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayMusic(AudioManager.Instance.backgroundMusic);
        }
    }

    public static void PlayMainMenuMusic()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayMusic(AudioManager.Instance.mainMenuMusic);
        }
    }

    public static void StopMusic()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.musicSource.Stop();
        }
    }

    public static void PlayFootstep()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(AudioManager.Instance.playerFootstep);
        }
    }

    public static void PlayDash()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(AudioManager.Instance.playerDash);
        }
    }

    public static void PlayPlayerAttack()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(AudioManager.Instance.playerAttack);
        }
    }

    public static void PlayPlayerDamage()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(AudioManager.Instance.playerTakeDamage);
        }
    }

    public static void PlayCorridorAppear()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(AudioManager.Instance.corridorAppear);
        }
    }

    public static void PlayIntroRevealSound()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(AudioManager.Instance.introRevealSound);
        }
    }

    public static void PlayButtonClick()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(AudioManager.Instance.buttonClick);
        }
    }

    public static void PlayBossHit(int bossNumber)
    {
        if (AudioManager.Instance != null)
        {
            switch (bossNumber)
            {
                case 1: AudioManager.Instance.PlaySFX(AudioManager.Instance.boss1Hit); break;
                case 2: AudioManager.Instance.PlaySFX(AudioManager.Instance.boss2Hit); break;
                case 3: AudioManager.Instance.PlaySFX(AudioManager.Instance.boss3Hit); break;
                default: Debug.LogWarning($"Звук для босса {bossNumber} не найден!"); break;
            }
        }
    }

    // --- Звуки анимации букв ---

    public static void PlayLetterStandard()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(AudioManager.Instance.letterMoveStandard);
        }
    }

    public static void PlayLetterFast()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(AudioManager.Instance.letterMoveFast);
        }
    }

    public static void PlayLetterImpact()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(AudioManager.Instance.letterMoveImpact);
        }
    }

    public static void PlayLetterRandom()
    {
        if (AudioManager.Instance != null && AudioManager.Instance.randomLetterSounds.Length > 0)
        {
            int randomIndex = Random.Range(0, AudioManager.Instance.randomLetterSounds.Length);
            AudioManager.Instance.PlaySFX(AudioManager.Instance.randomLetterSounds[randomIndex]);
        }
    }
}