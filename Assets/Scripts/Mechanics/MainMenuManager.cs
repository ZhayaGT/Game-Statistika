using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using DG.Tweening;

public class MainMenuManager : MonoBehaviour
{
    [Header("Scene Navigation")]
    [SerializeField] private string nextSceneName = "Level1";
    [SerializeField] private Button playButton;
    [SerializeField] private string playerPrefLastLevelKey = "LastUnlockedLevel"; // Key for storing last level
    
    [Header("Fade Transition")]
    [SerializeField] private Image fadeImage;
    [SerializeField] private float fadeDuration = 1f;
    
    [Header("Profile UI")]
    [SerializeField] private GameObject profilePanel;
    [SerializeField] private Button profileButton;
    [SerializeField] private Button hideProfileButton;
    
    [Header("Instructions UI")]
    [SerializeField] private GameObject instructionsPanel;
    [SerializeField] private Button instructionsButton;
    [SerializeField] private Button hideInstructionsButton;
    
    [Header("Exit")]
    [SerializeField] private Button exitButton;
    
    [Header("Animation Settings")]
    [SerializeField] private float popupDuration = 0.5f;
    [SerializeField] private Ease popupEase = Ease.OutBack;
    [SerializeField] private float startScale = 0.1f;
    
    private void Awake()
    {
        // Initialize fade image
        if (fadeImage != null)
        {
            // Make sure fade image is transparent at start
            Color fadeColor = fadeImage.color;
            fadeColor.a = 0f;
            fadeImage.color = fadeColor;
            fadeImage.gameObject.SetActive(true);
        }
        
        // Hide profile panel at start
        if (profilePanel != null)
        {
            profilePanel.SetActive(false);
        }
        
        // Hide instructions panel at start
        if (instructionsPanel != null)
        {
            instructionsPanel.SetActive(false);
        }
    }
    
    private void Start()
    {
        // Set up button listeners
        if (playButton != null)
        {
            playButton.onClick.AddListener(OnPlayButtonClicked);
        }
        
        if (profileButton != null)
        {
            profileButton.onClick.AddListener(ShowProfilePanel);
        }
        
        if (hideProfileButton != null)
        {
            hideProfileButton.onClick.AddListener(HideProfilePanel);
        }
        
        if (instructionsButton != null)
        {
            instructionsButton.onClick.AddListener(ShowInstructionsPanel);
        }
        
        if (hideInstructionsButton != null)
        {
            hideInstructionsButton.onClick.AddListener(HideInstructionsPanel);
        }
        
        if (exitButton != null)
        {
            exitButton.onClick.AddListener(ExitGame);
        }
    }
    
    private void OnPlayButtonClicked()
    {
        // Check if we have a saved level
        int savedLevel = PlayerPrefs.GetInt(playerPrefLastLevelKey, 0);
        
        if (savedLevel > 0)
        {
            // Load the saved level
            string sceneName = SceneUtility.GetScenePathByBuildIndex(savedLevel);
            if (!string.IsNullOrEmpty(sceneName))
            {
                // Extract just the scene name from the path
                sceneName = System.IO.Path.GetFileNameWithoutExtension(sceneName);
                FadeAndLoadScene(sceneName);
                return;
            }
        }
        
        // If no saved level or invalid, load the default next scene
        FadeAndLoadScene(nextSceneName);
    }
    
    private void FadeAndLoadScene(string sceneName)
    {
        if (fadeImage != null)
        {
            // Fade to black
            fadeImage.DOFade(1f, fadeDuration)
                .SetEase(Ease.InOutQuad)
                .OnComplete(() => {
                    // Load the scene after fade completes
                    SceneManager.LoadScene(sceneName);
                });
        }
        else
        {
            // No fade image, just load the scene
            SceneManager.LoadScene(sceneName);
        }
    }
    
    private void ShowProfilePanel()
    {
        if (profilePanel != null)
        {
            // Hide instructions panel if it's active
            if (instructionsPanel != null && instructionsPanel.activeSelf)
            {
                HideInstructionsPanel();
            }
            
            // Make panel active but start small
            profilePanel.SetActive(true);
            profilePanel.transform.localScale = new Vector3(startScale, startScale, startScale);
            
            // Animate scale up
            profilePanel.transform.DOScale(1f, popupDuration)
                .SetEase(popupEase);
        }
    }
    
    private void HideProfilePanel()
    {
        if (profilePanel != null)
        {
            // Animate scale down
            profilePanel.transform.DOScale(startScale, popupDuration)
                .SetEase(Ease.InBack)
                .OnComplete(() => {
                    profilePanel.SetActive(false);
                });
        }
    }
    
    private void ShowInstructionsPanel()
    {
        if (instructionsPanel != null)
        {
            // Hide profile panel if it's active
            if (profilePanel != null && profilePanel.activeSelf)
            {
                HideProfilePanel();
            }
            
            // Make panel active but start small
            instructionsPanel.SetActive(true);
            instructionsPanel.transform.localScale = new Vector3(startScale, startScale, startScale);
            
            // Animate scale up
            instructionsPanel.transform.DOScale(1f, popupDuration)
                .SetEase(popupEase);
        }
    }
    
    private void HideInstructionsPanel()
    {
        if (instructionsPanel != null)
        {
            // Animate scale down
            instructionsPanel.transform.DOScale(startScale, popupDuration)
                .SetEase(Ease.InBack)
                .OnComplete(() => {
                    instructionsPanel.SetActive(false);
                });
        }
    }
    
    private void ExitGame()
    {
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }
    
    private void OnDestroy()
    {
        // Kill all tweens when destroyed to prevent memory leaks
        DOTween.Kill(fadeImage);
        if (profilePanel != null)
        {
            DOTween.Kill(profilePanel.transform);
        }
        if (instructionsPanel != null)
        {
            DOTween.Kill(instructionsPanel.transform);
        }
    }
}