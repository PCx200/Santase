using System.Collections.Generic;
using UnityEngine;

public class HandView : MonoBehaviour
{
    [SerializeField] Transform player1HandRoot;
    [SerializeField] Transform player2HandRoot;
    [SerializeField] List<Card_Presenter> cardPresenters;

    public void UpdateHand(int playerID, List<Card> hand)
    {
        Transform root = playerID == 0 ? player1HandRoot : player2HandRoot;

        foreach (Transform child in root)
            Destroy(child.gameObject);

        for (int i = 0; i < hand.Count; i++)
        {
            Card card = hand[i];

            Card_Presenter prefab = cardPresenters.Find(cp =>
                cp.card_SO.Name == card.GetName() &&
                cp.card_SO.Suit == card.GetSuit()
            );

            Card_Presenter presenter = Instantiate(prefab, root);
            presenter.card = card;

            var click = presenter.gameObject.AddComponent<CardClickController>();
            click.cardIndex = i;
        }
    }
}
