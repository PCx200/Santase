using UnityEngine;
using TMPro;

public class TurnView : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI turnText;

    public void UpdateTurn(int playerID)
    {
        turnText.text = playerID == 0 ? "Player 1 Turn" : "Player 2 Turn";
    }
}
