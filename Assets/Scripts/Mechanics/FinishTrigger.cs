using UnityEngine;
using Platformer.Gameplay;
using static Platformer.Core.Simulation;

public class FinishTrigger : MonoBehaviour
{
    [SerializeField] private string playerTag = "Player";
    
    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.CompareTag(playerTag))
        {
            // Show the results UI regardless of questions answered
            if (QuizUIManager.Instance != null)
            {
                // This will show the results UI even if not all questions are answered
                QuizUIManager.Instance.ShowResults();
            }
        }
    }
}