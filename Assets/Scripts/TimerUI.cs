using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TimerUI : MonoBehaviour
{
    public LevelManager levelManager;
    public TextMeshProUGUI timerText;
    public Slider timerSlider;

    void Update()
    {
        if (levelManager != null)
        {
            float t = Mathf.Max(0, levelManager.GetTimer());
            timerText.text = $"Temps : {t:F1}";
            if (timerSlider != null)
            {
                timerSlider.value = t / levelManager.GetMaxTimer();
            }
        }
    }
}
