using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayedCardView : MonoBehaviour
{
    [SerializeField] Image player1Spot;
    [SerializeField] Image player2Spot;
    [SerializeField] List<Card_Presenter> cardPresenters;

    public void ShowCard(Card card, int playerID)
    {
        Card_Presenter prefab = cardPresenters.Find(cp =>
            cp.card_SO.Name == card.GetName() &&
            cp.card_SO.Suit == card.GetSuit()
        );

        Image target = playerID == 0 ? player1Spot : player2Spot;

        target.sprite = prefab.card_SO.sprite;
        var c = target.color;
        c.a = 1;
        target.color = c;
    }
}
