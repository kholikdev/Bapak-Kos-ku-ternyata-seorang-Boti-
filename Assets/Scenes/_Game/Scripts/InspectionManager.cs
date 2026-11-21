using UnityEngine;
using TMPro;

public class InspectionManager : MonoBehaviour
{
    [Header("Timer Settings")]
    public float timeRemaining = 60f;
    public TextMeshProUGUI timerText;
    public EndingEvaluator endingEvaluator;

    [Header("Audio Settings")]
    public AudioSource timerAudioSource;
    public AudioClip tickSound;
    [Range(0f, 1f)] public float tickVolume = 0.6f;

    private bool isTimerRunning = true;
    private int lastSecondRecorded = -1;

    void Start()
    {
        if (timerAudioSource == null)
        {
            timerAudioSource = GetComponent<AudioSource>();
            if (timerAudioSource == null)
            {
                timerAudioSource = gameObject.AddComponent<AudioSource>();
                timerAudioSource.playOnAwake = false;
                timerAudioSource.spatialBlend = 0f;
            }
        }
    }

    void Update()
    {
        if (!isTimerRunning) return;

        // hitung mundur
        if (timeRemaining > 0)
        {
            timeRemaining -= Time.deltaTime;
            UpdateTimerUI(timeRemaining);

            // Suara tik satu kali setiap angka detik berganti
            int currentSecond = Mathf.FloorToInt(timeRemaining);
            if (currentSecond != lastSecondRecorded)
            {
                lastSecondRecorded = currentSecond;

                if (timerAudioSource != null && tickSound != null)
                {
                    timerAudioSource.PlayOneShot(tickSound, tickVolume);
                }
                else if (AudioManager.Instance != null && tickSound != null)
                {
                    AudioManager.Instance.PlaySFX(tickSound, tickVolume);
                }
            }
        }
        else
        {
            timeRemaining = 0;
            isTimerRunning = false;
            UpdateTimerUI(0);
            TriggerInspection();
        }
    }

    void UpdateTimerUI(float timeToDisplay)
    {
        float minutes = Mathf.FloorToInt(timeToDisplay / 60);
        float seconds = Mathf.FloorToInt(timeToDisplay % 60);
        timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    void TriggerInspection()
    {
        // mulai inspeksi
        if (endingEvaluator != null)
            endingEvaluator.StartCutscene();
    }
}
