using UnityEngine;
using TMPro;

public class TurnView : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI turnText1;
    [SerializeField] private TextMeshProUGUI turnText2;

    public void UpdateTurn(int playerID)
    {
        GetTurnSpot(0).text = playerID == 0 ? "YOUR TURN" : "";
        GetTurnSpot(1).text = playerID == 1 ? "YOUR TURN" : "";
    }

    private TextMeshProUGUI GetTurnSpot(int playerID)
    {
        if (GameController.Instance.localPlayerID == 0) return playerID == 0 ? turnText1 : turnText2;

        else return playerID == 1 ? turnText1 : turnText2;
    }
}
