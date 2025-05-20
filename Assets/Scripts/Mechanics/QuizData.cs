using UnityEngine;

[CreateAssetMenu(fileName = "New Quiz", menuName = "Quiz Game/Quiz Data")]
public class QuizData : ScriptableObject
{
    [Header("Quiz Question")]
    public Sprite questionImage;
    [TextArea(3, 5)]
    public string questionText;
    
    [Header("Answer Options")]
    public string[] answerOptions = new string[4];
    public int correctAnswerIndex;
    public Sprite solutionImage;
}