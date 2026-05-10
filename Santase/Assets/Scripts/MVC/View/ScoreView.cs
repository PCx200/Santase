using UnityEngine;
using TMPro;

public class ScoreView : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI p1Score;
    [SerializeField] TextMeshProUGUI p2Score;

    [SerializeField] TextMeshProUGUI player1Result;
    [SerializeField] TextMeshProUGUI player2Result;

    public void UpdateRoundScore(int p1, int p2)
    {
        p1Score.text = p1.ToString();
        p2Score.text = p2.ToString();
    }

    public void UpdateGameScore(int p1, int p2)
    {
        player1Result.text = p1.ToString();
        player2Result.text = p2.ToString();
    }
}
