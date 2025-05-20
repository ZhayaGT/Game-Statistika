using UnityEngine;

public class QuizTrigger : MonoBehaviour
{
    [SerializeField] private QuizData quizData;
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private bool destroyAfterTriggered = true;
    
    private bool hasBeenTriggered = false;
    
    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (hasBeenTriggered) return;
        
        if (collider.CompareTag(playerTag))
        {
            hasBeenTriggered = true;
            
            // Show the quiz UI
            QuizUIManager.Instance.ShowQuiz(quizData);
            
            if (destroyAfterTriggered)
            {
                // Optionally destroy the trigger after it's been activated
                Destroy(gameObject);
            }
        }
    }
}
