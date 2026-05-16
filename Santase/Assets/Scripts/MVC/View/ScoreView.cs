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
        GetRoundScoreSpot(0).text = p1.ToString();
        GetRoundScoreSpot(1).text = p2.ToString();
    }

    public void UpdateGameScore(int p1, int p2)
    {
        GetGameScoreSpot(0).text = p1.ToString();
        GetGameScoreSpot(1).text = p2.ToString();
    }

    private TextMeshProUGUI GetRoundScoreSpot(int playerID)
    {
        if (FindFirstObjectByType<GameController>().localPlayerID == 0) return playerID == 0 ? p1Score : p2Score;

        else return playerID == 1 ? p1Score : p2Score;
    }

    private TextMeshProUGUI GetGameScoreSpot(int playerID)
    {
        if (FindFirstObjectByType<GameController>().localPlayerID == 0) return playerID == 0 ? player1Result : player2Result;

        else return playerID == 1 ? player1Result : player2Result;
    }
}
