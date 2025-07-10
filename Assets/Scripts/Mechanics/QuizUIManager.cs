using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening; // Add DOTween namespace

public class QuizUIManager : MonoBehaviour
{
    public static QuizUIManager Instance { get; private set; }
    
    [Header("Material Panel")]
    [SerializeField] private GameObject materialPanel;
    [SerializeField] private Button closeMaterialButton;
    [SerializeField] private GameObject controllerUI; // Reference to your controller UI
    
    [Header("Quiz Panel")]
    [SerializeField] private GameObject quizPanel;
    [SerializeField] private Image questionImage;
    [SerializeField] private TextMeshProUGUI questionText;
    [SerializeField] private Button[] answerButtons = new Button[4];
    [SerializeField] private TextMeshProUGUI[] answerTexts = new TextMeshProUGUI[4];
    
    [Header("Results Panel")]
    [SerializeField] private GameObject resultsPanel;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private Button nextLevelButton;
    [SerializeField] private Button mainMenuButton; // New button for returning to main men
    [SerializeField] private string mainMenuSceneName = "Main Menu"; // Scene name to load
    [SerializeField] private string playerPrefLastLevelKey = "LastUnlockedLevel"; // Key for storing last level
    [SerializeField] private GameObject perfectScoreObject; // Object to show when score is 100
    
    [Header("Feedback Panels")]
    [SerializeField] private GameObject correctPanel;
    [SerializeField] private TextMeshProUGUI correctAnswerText;
    [SerializeField] private Button correctContinueButton;
    
    [SerializeField] private GameObject wrongPanel;
    [SerializeField] private TextMeshProUGUI wrongCorrectAnswerText;
    [SerializeField] private Image solutionImage;
    [SerializeField] private Button wrongContinueButton;
    
    [Header("Animation Settings")]
    [SerializeField] private float popupDuration = 0.5f;
    [SerializeField] private Ease popupEase = Ease.OutBack;
    [SerializeField] private float startScale = 0.1f;
    
    [Header("Audio Settings")]
    [SerializeField] private AudioClip correctAnswerSound;
    [SerializeField] private AudioClip wrongAnswerSound;
    private AudioSource audioSource;
    
    private QuizData currentQuiz;
    
    [Header("Player References")]
    [SerializeField] private Platformer.Mechanics.PlayerController playerController;
    
    [Header("In-Game UI")]
    [SerializeField] private Button inGameMainMenuButton; // Button for returning to main menu during gameplay
    
    void Awake()
    {
        // Simple singleton pattern without DontDestroyOnLoad
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        // Add audio source if not present
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        // Find player controller if not assigned
        if (playerController == null)
        {
            playerController = FindObjectOfType<Platformer.Mechanics.PlayerController>();
        }
        
        // Hide panels at start
        quizPanel.SetActive(false);
        resultsPanel.SetActive(false);
        correctPanel.SetActive(false);
        wrongPanel.SetActive(false);
        materialPanel.SetActive(false); // Hide material panel initially
    }
    
    void Start()
    {
        // Subscribe to events
        if (QuizManager.Instance != null)
        {
            // Remove this line to prevent automatic showing of results
            // QuizManager.Instance.OnAllQuizzesCompleted += ShowResults;
        }
        
        // Set up material panel close button
        if (closeMaterialButton != null)
        {
            closeMaterialButton.onClick.AddListener(CloseMaterialPanel);
        }
        
        // Set up next level button
        nextLevelButton.onClick.AddListener(() => {
            CloseWithAnimation(resultsPanel, () => {
                // Unpause the game when next level button is clicked
                Time.timeScale = 1f;
                
                if (QuizManager.Instance != null)
                {
                    // Save current level index to PlayerPrefs before loading next level
                    int currentSceneIndex = UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex;
                    int nextSceneIndex = currentSceneIndex + 1;
                    PlayerPrefs.SetInt(playerPrefLastLevelKey, nextSceneIndex);
                    PlayerPrefs.Save();
                    
                    QuizManager.Instance.LoadNextLevel();
                }
            });
        });
        
        // Set up main menu button
        if (mainMenuButton != null)
        {
            mainMenuButton.onClick.AddListener(() => {
                CloseWithAnimation(resultsPanel, () => {
                    // Unpause the game when main menu button is clicked
                    Time.timeScale = 1f;
                    
                    // Load the main menu scene
                    UnityEngine.SceneManagement.SceneManager.LoadScene(mainMenuSceneName);
                });
            });
        }
        
        // Set up in-game main menu button
        if (inGameMainMenuButton != null)
        {
            inGameMainMenuButton.onClick.AddListener(() => {
                // Unpause the game when main menu button is clicked
                Time.timeScale = 1f;
                
                // Load the main menu scene (index 0)
                UnityEngine.SceneManagement.SceneManager.LoadScene(0);
            });
        }
        
        // Set up continue buttons for feedback panels
        correctContinueButton.onClick.AddListener(() => {
            CloseWithAnimation(correctPanel, () => {
                // Show controller UI when closing the panel
                if (controllerUI != null)
                {
                    controllerUI.SetActive(true);
                }
                
                Time.timeScale = 1f;
            });
        });
        
        wrongContinueButton.onClick.AddListener(() => {
            CloseWithAnimation(wrongPanel, () => {
                // Show controller UI when closing the panel
                if (controllerUI != null)
                {
                    controllerUI.SetActive(true);
                }
                
                Time.timeScale = 1f;
            });
        });
        
        // Show material panel at start with animation
        ShowMaterialPanel();
    }
    
    // Show material panel and hide controller UI
    public void ShowMaterialPanel()
    {
        // Hide controller UI
        if (controllerUI != null)
        {
            controllerUI.SetActive(false);
        }
        
        // Show material panel with animation
        ShowWithAnimation(materialPanel);
        
        // Pause the game while showing material
        Time.timeScale = 0f;
    }
    
    // Close material panel and show controller UI
    public void CloseMaterialPanel()
    {
        CloseWithAnimation(materialPanel, () => {
            // Show controller UI
            if (controllerUI != null)
            {
                controllerUI.SetActive(true);
            }
            
            // Resume the game
            Time.timeScale = 1f;
        });
    }
    
    public void ShowQuiz(QuizData quizData)
    {
        // If material panel is active, close it first
        if (materialPanel.activeSelf)
        {
            CloseWithAnimation(materialPanel, () => {
                ShowQuizInternal(quizData);
            });
        }
        else
        {
            ShowQuizInternal(quizData);
        }
    }
    
    private void ShowQuizInternal(QuizData quizData)
    {
        // Stop player movement
        if (playerController != null)
        {
            playerController.StopMoving();
        }
        
        currentQuiz = quizData;
        
        // Set question
        questionImage.sprite = quizData.questionImage;
        questionText.text = quizData.questionText;
        
        // Set answer options
        for (int i = 0; i < answerButtons.Length; i++)
        {
            int index = i; // Capture the index for the lambda
            answerTexts[i].text = quizData.answerOptions[i];
            
            // Clear previous listeners and add new one
            answerButtons[i].onClick.RemoveAllListeners();
            answerButtons[i].onClick.AddListener(() => AnswerSelected(index));
        }
        
        // Hide controller UI when showing quiz
        if (controllerUI != null)
        {
            controllerUI.SetActive(false);
        }
        
        // Show the quiz panel with animation
        ShowWithAnimation(quizPanel);
        
        // Pause game
        Time.timeScale = 0f;
    }
    
    private void AnswerSelected(int selectedIndex)
    {
        bool isCorrect = (selectedIndex == currentQuiz.correctAnswerIndex);
        
        // Hide the quiz panel with animation
        CloseWithAnimation(quizPanel, () => {
            // Show appropriate feedback panel
            if (isCorrect)
            {
                ShowCorrectPanel();
                // Play correct sound
                if (correctAnswerSound != null)
                {
                    audioSource.PlayOneShot(correctAnswerSound);
                }
            }
            else
            {
                ShowWrongPanel();
                // Play wrong sound
                if (wrongAnswerSound != null)
                {
                    audioSource.PlayOneShot(wrongAnswerSound);
                }
            }
            
            // Notify the QuizManager about the answer
            if (QuizManager.Instance != null)
            {
                QuizManager.Instance.AnswerQuestion(isCorrect);
            }
        });
    }
    
    private void ShowCorrectPanel()
    {
        // Stop player movement
        if (playerController != null)
        {
            playerController.StopMoving();
        }
        
        // Set the correct answer text
        correctAnswerText.text = currentQuiz.answerOptions[currentQuiz.correctAnswerIndex];
        
        // Show the correct panel with animation
        ShowWithAnimation(correctPanel);
        
        // Keep the game paused
        Time.timeScale = 0f;
    }
    
    private void ShowWrongPanel()
    {
        // Stop player movement
        if (playerController != null)
        {
            playerController.StopMoving();
        }
        
        // Set the correct answer text
        wrongCorrectAnswerText.text = currentQuiz.answerOptions[currentQuiz.correctAnswerIndex];
        
        // Set the solution image
        solutionImage.sprite = currentQuiz.solutionImage;
        
        // Set the image to native size to prevent stretching
        solutionImage.preserveAspect = true;
        
        // Optional: If you want to exactly match the native size
        if (currentQuiz.solutionImage != null)
        {
            RectTransform rectTransform = solutionImage.rectTransform;
            rectTransform.sizeDelta = new Vector2(currentQuiz.solutionImage.rect.width, currentQuiz.solutionImage.rect.height);
        }
        
        // Show the wrong panel with animation
        ShowWithAnimation(wrongPanel);
        
        // Keep the game paused
        Time.timeScale = 0f;
    }
    
    public void ShowResults()
    {
        int currentScore = 0;
        
        // Update score text
        if (QuizManager.Instance != null)
        {
            currentScore = QuizManager.Instance.GetCurrentScore();
            scoreText.text = currentScore.ToString();
            
            // Save the current level's score to PlayerPrefs
            int sceneIndex = UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex;
            PlayerPrefs.SetInt($"Level{sceneIndex}Score", currentScore);
            PlayerPrefs.Save();
        }
        
        // Show or hide perfect score object based on score
        if (perfectScoreObject != null)
        {
            perfectScoreObject.SetActive(currentScore == 100);
        }
        
        // Save current level progress to PlayerPrefs
        int currentSceneIndex = UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex;
        int savedLevel = PlayerPrefs.GetInt(playerPrefLastLevelKey, 0);
        
        // Only update if current level is higher than saved level
        if (currentSceneIndex > savedLevel)
        {
            PlayerPrefs.SetInt(playerPrefLastLevelKey, currentSceneIndex + 1);
            PlayerPrefs.Save();
        }
        
        // Hide controller UI when showing results
        if (controllerUI != null)
        {
            controllerUI.SetActive(false);
        }
        
        // Show the results panel with animation
        ShowWithAnimation(resultsPanel);
        
        // Pause the game
        Time.timeScale = 0f;
    }
    
    // Call this method from a finish trigger
    public void ShowFinalResults()
    {
        // Only show results if all quizzes are completed
        if (QuizManager.Instance != null && 
            QuizManager.Instance.GetAnsweredQuizzes() >= QuizManager.Instance.GetTotalQuizzes())
        {
            ShowResults();
        }
        else
        {
            // Optional: Show a message that player needs to answer all questions
            Debug.Log("Player reached finish line but hasn't answered all questions yet");
            
            // You could show a different UI panel here to inform the player
            // that they need to complete all quizzes before finishing
        }
    }
    
    // Animation helper methods
    private void ShowWithAnimation(GameObject panel)
    {
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