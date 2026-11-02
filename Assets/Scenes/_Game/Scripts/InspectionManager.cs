using UnityEngine;
using TMPro;

public class InspectionManager : MonoBehaviour
{
    [Header("Timer Settings")]
    public float timeRemaining = 60f;
    public TextMeshProUGUI timerText;
    public EndingEvaluator endingEvaluator;

    private bool isTimerRunning = true;

    void Update()
    {
        if (isTimerRunning)
        {
            if (timeRemaining > 0)
            {
                timeRemaining -= Time.deltaTime;
                UpdateTimerUI(timeRemaining);
            }
            else
            {
                timeRemaining = 0;
                isTimerRunning = false;
                UpdateTimerUI(0);
                TriggerInspection();
            }
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
        Debug.Log("Waktu Habis! Cutscene Timeline Dimulai...");
        if (endingEvaluator != null)
        {
            endingEvaluator.StartCutscene();
        }
    }
}