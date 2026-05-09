using UnityEngine;
using UnityEngine.UI;

public class PlayedCardResetController : MonoBehaviour
{
    [SerializeField] Image player1Spot;
    [SerializeField] Image player2Spot;

    public void ResetSpots()
    {
        var c1 = player1Spot.color; c1.a = 0; player1Spot.color = c1;
        var c2 = player2Spot.color; c2.a = 0; player2Spot.color = c2;
    }
}
