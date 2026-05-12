using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayedCardView : MonoBehaviour
{
    [SerializeField] Image player1Spot;
    [SerializeField] Image player2Spot;
    [SerializeField] Sprite backFace;
    [SerializeField] List<CardPresenter> cardPresenters;

    public void ShowCard(int playerID, Card card)
    {
        CardPresenter prefab = cardPresenters.Find(cp =>
            cp.card_SO.Name == card.GetName() &&
            cp.card_SO.Suit == card.GetSuit()
        );

        Image target = playerID == 0 ? player1Spot : player2Spot;

        target.sprite = prefab.card_SO.sprite;
        var c = target.color;
        c.a = 1;
        target.color = c;
    }

    public IEnumerator ResetAfterTrick()
    {
        yield return new WaitForSeconds(0.5f);
        player1Spot.sprite = backFace;
        player2Spot.sprite = backFace;

        var c = player1Spot.color;
        c.a = 0.1f;

        player1Spot.color = c;
        player2Spot.color = c;
    }
}
