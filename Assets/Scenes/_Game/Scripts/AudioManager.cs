using UnityEngine;
using UnityEngine.InputSystem;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Sources")]
    public AudioSource bgmSource;
    public AudioSource sfxSource;

    [Header("Default BGM Clips")]
    public AudioClip gameplayBGM;
    public AudioClip tenseCutsceneBGM;

    [Header("Volume Controls")]
    [Range(0f, 1f)] public float bgmVolume = 1.0f;
    [Range(0f, 1f)] public float sfxVolume = 1.0f;

    [Header("Mute Controls (M = Mute BGM, N = Mute SFX)")]
    public bool isBGMMuted = false;
    public bool isSFXMuted = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // Jika audio source belum di-assign di inspector, setup otomatis
            SetupAudioSources();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        if (gameplayBGM != null)
        {
            PlayBGM(gameplayBGM);
        }
    }

    void Update()
    {
        // Tombol M untuk mute/unmute backsound, N untuk mute/unmute sound effects
        if (Keyboard.current != null)
        {
            if (Keyboard.current.mKey.wasPressedThisFrame)
            {
                ToggleBGMMute();
            }

            if (Keyboard.current.nKey.wasPressedThisFrame)
            {
                ToggleSFXMute();
            }
        }
    }

    public void ToggleBGMMute()
    {
        isBGMMuted = !isBGMMuted;
        if (bgmSource != null)
        {
            bgmSource.mute = isBGMMuted;
        }
    }

    public void ToggleSFXMute()
    {
        isSFXMuted = !isSFXMuted;
        if (sfxSource != null)
        {
            sfxSource.mute = isSFXMuted;
        }

        // Mute / unmute semua AudioSource SFX di dalam scene
        AudioSource[] allSources = FindObjectsByType<AudioSource>(FindObjectsSortMode.None);
        foreach (var src in allSources)
        {
            if (src != null && src != bgmSource)
            {
                src.mute = isSFXMuted;
            }
        }
    }

    private void SetupAudioSources()
    {
        AudioSource[] sources = GetComponents<AudioSource>();
        if (sources.Length >= 2)
        {
            if (bgmSource == null) bgmSource = sources[0];
            if (sfxSource == null) sfxSource = sources[1];
        }
        else
        {
            if (bgmSource == null) bgmSource = gameObject.AddComponent<AudioSource>();
            if (sfxSource == null) sfxSource = gameObject.AddComponent<AudioSource>();
        }

        bgmSource.loop = true;
        bgmSource.playOnAwake = false;
        bgmSource.volume = bgmVolume;

        sfxSource.loop = false;
        sfxSource.playOnAwake = false;
        sfxSource.volume = sfxVolume;
        sfxSource.ignoreListenerPause = true;
    }

    public void PlayBGM(AudioClip clip, bool loop = true)
    {
        if (clip == null || bgmSource == null) return;

        bgmSource.clip = clip;
        bgmSource.loop = loop;
        bgmSource.volume = bgmVolume;
        bgmSource.mute = isBGMMuted;
        bgmSource.Play();
    }

    public void SwitchToTenseBGM()
    {
        if (tenseCutsceneBGM != null)
        {
            PlayBGM(tenseCutsceneBGM, true);
        }
    }

    public void StopBGM()
    {
        if (bgmSource != null)
        {
            bgmSource.Stop();
        }
    }

    public void PlaySFX(AudioClip clip, float volumeScale = 1.0f)
    {
        if (isSFXMuted) return;
        if (clip == null || sfxSource == null) return;

        sfxSource.PlayOneShot(clip, volumeScale * sfxVolume);
    }
}
