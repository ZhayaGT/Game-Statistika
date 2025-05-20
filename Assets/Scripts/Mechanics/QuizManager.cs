using UnityEngine;
using UnityEngine.SceneManagement;

public class QuizManager : MonoBehaviour
{
    public static QuizManager Instance { get; private set; }
    
    [SerializeField] private int maxScore = 100;
    
    private int currentScore = 0;
    private int answeredQuizzes = 0;
    private int totalQuizzesInLevel = 5; // Default value
    private int scorePerCorrectAnswer;
    
    // Event that can be subscribed to when score changes
    public delegate void ScoreChangedDelegate(int newScore);
    public event ScoreChangedDelegate OnScoreChanged;
    
    // Event that can be subscribed to when all quizzes are completed
    public delegate void AllQuizzesCompletedDelegate();
    public event AllQuizzesCompletedDelegate OnAllQuizzesCompleted;
    
    void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void Start()
    {
        ResetScore();
        SetupLevelQuizCount();
    }
    
    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    
    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        SetupLevelQuizCount();
    }
    
    private void SetupLevelQuizCount()
    {
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        
        // Set the number of quizzes based on the scene index
        switch (currentSceneIndex)
        {
            case 1:
                totalQuizzesInLevel = 5;
                break;
            case 2:
                totalQuizzesInLevel = 10;
                break;
            case 3:
                totalQuizzesInLevel = 5;
                break;
            case 4:
                totalQuizzesInLevel = 15;
                break;
            case 5:
                totalQuizzesInLevel = 5;
                break;
            default:
                totalQuizzesInLevel = 5; // Default value for other scenes
                break;
        }
        
        // Calculate score per correct answer based on total quizzes
        scorePerCorrectAnswer = totalQuizzesInLevel > 0 ? maxScore / totalQuizzesInLevel : 0;
        
        // Reset counters when a new scene is loaded
        ResetScore();
        
        Debug.Log($"Level {currentSceneIndex} has {totalQuizzesInLevel} quizzes. Each correct answer is worth {scorePerCorrectAnswer} points.");
    }
    
    public void AnswerQuestion(bool isCorrect)
    {
        if (isCorrect)
        {
            currentScore += scorePerCorrectAnswer;
            // Make sure score doesn't exceed max
            currentScore = Mathf.Min(currentScore, maxScore);
        }
        
        answeredQuizzes++;
        
        // Notify subscribers that score has changed
        OnScoreChanged?.Invoke(currentScore);
        
        // Check if all quizzes in the level are completed
        if (answeredQuizzes >= totalQuizzesInLevel)
        {
            OnAllQuizzesCompleted?.Invoke();
        }
    }
    
    public int GetCurrentScore()
    {
        return currentScore;
    }
    
    public int GetTotalQuizzes()
    {
        return totalQuizzesInLevel;
    }
    
    public int GetAnsweredQuizzes()
    {
        return answeredQuizzes;
    }
    
    public void ResetScore()
    {
        currentScore = 0;
        answeredQuizzes = 0;
        OnScoreChanged?.Invoke(currentScore);
    }
    
    public void LoadNextLevel()
    {
        ResetScore();
        int nextSceneIndex = SceneManager.GetActiveScene().buildIndex + 1;
        if (nextSceneIndex < SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadScene(nextSceneIndex);
        }
        else
        {
            // Game completed, could load a final scene or return to menu
            SceneManager.LoadScene(0);
        }
    }
}
