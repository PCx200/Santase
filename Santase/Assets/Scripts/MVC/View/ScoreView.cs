using UnityEngine;
using TMPro;

public class ScoreView : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI p1Score;
    [SerializeField] TextMeshProUGUI p2Score;

    public void UpdateScore(int p1, int p2)
    {
        p1Score.text = p1.ToString();
        p2Score.text = p2.ToString();
    }
}
