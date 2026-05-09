using UnityEngine;
using TMPro;

public class RoundOverView : MonoBehaviour
{
    [SerializeField] GameObject panel;
    [SerializeField] TextMeshProUGUI winnerText;

    public void ShowWinner(int playerID)
    {
        panel.SetActive(true);
        winnerText.text = playerID == 0 ? "Player 1 Wins!" : "Player 2 Wins!";
    }

    public void DisablePanel()
    {
        panel.SetActive(false);
    }
}