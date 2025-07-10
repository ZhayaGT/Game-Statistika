using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class AverageScoreDisplay : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject averageScorePanel;
    [SerializeField] private TextMeshProUGUI level1ScoreText;
    [SerializeField] private TextMeshProUGUI level2ScoreText;
    [SerializeField] private TextMeshProUGUI averageScoreText;
    [SerializeField] private Button continueButton;
    
    [Header("Animation Settings")]
    [SerializeField] private float popupDuration = 0.5f;
    [SerializeField] private Ease popupEase = Ease.OutBack;
    [SerializeField] private float startScale = 0.1f;
    
    private void Start()
    {
        // Hide panel initially
        if (averageScorePanel != null)
        {
            averageScorePanel.SetActive(false);
        }
        
        // Set up continue button
        if (continueButton != null)
        {
            continueButton.onClick.AddListener(CloseAverageScorePanel);
        }
        
        // Show the average score panel with animation
        ShowAverageScorePanel();
    }
    
    public void ShowAverageScorePanel()
    {
        // Get scores from PlayerPrefs
        int level1Score = PlayerPrefs.GetInt("Level1Score", 0);
        int level2Score = PlayerPrefs.GetInt("Level2Score", 0);
        
        // Calculate average
        float averageScore = (level1Score + level2Score) / 2f;
        
        // Update UI texts
        if (level1ScoreText != null) level1ScoreText.text = level1Score.ToString();
        if (level2ScoreText != null) level2ScoreText.text = level2Score.ToString();
        if (averageScoreText != null) averageScoreText.text = averageScore.ToString("F1"); // One decimal place
        
        // Show the panel with animation
        ShowWithAnimation(averageScorePanel);
        
        // Pause the game while showing the panel
        Time.timeScale = 0f;
    }
    
    public void CloseAverageScorePanel()
    {
        CloseWithAnimation(averageScorePanel, () => {
            // Resume the game
            Time.timeScale = 1f;
        });
    }
    
    // Animation helper methods
    private void ShowWithAnimation(GameObject panel)
    {
        if (panel == null) return;
        
        // Make sure the panel is active but scaled to 0
        panel.SetActive(true);
        panel.transform.localScale = new Vector3(startScale, startScale, startScale);
        
        // Animate the panel scaling up
        panel.transform.DOScale(1f, popupDuration)
            .SetEase(popupEase)
            .SetUpdate(true); // Make sure animation works even when game is paused
    }
    
    private void CloseWithAnimation(GameObject panel, TweenCallback onComplete = null)
    {
        if (panel == null) return;
        
        // Animate the panel scaling down
        panel.transform.DOScale(startScale, popupDuration)
            .SetEase(Ease.InBack)
            .SetUpdate(true) // Make sure animation works even when game is paused
            .OnComplete(() => {
                panel.SetActive(false);
                onComplete?.Invoke();
            });
    }
}