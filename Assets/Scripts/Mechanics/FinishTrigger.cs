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
            // Show the results UI
            if (QuizUIManager.Instance != null)
            {
                QuizUIManager.Instance.ShowFinalResults();
            }
        }
    }
}