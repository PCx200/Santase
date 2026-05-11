using System.Collections.Generic;
using UnityEngine;

public class HandView : MonoBehaviour
{
    [SerializeField] Transform player1HandRoot;
    [SerializeField] Transform player2HandRoot;
    [SerializeField] List<Card_Presenter> cardPresenters;
    [SerializeField] private Card_Presenter cardBack;

    public int localPlayerID;

    public void UpdateHand(int playerID, List<Card> hand)
    {
        Transform root = playerID == 0 ? player1HandRoot : player2HandRoot;

        foreach (Transform child in root)
            Destroy(child.gameObject);

        bool isLocal = (playerID == localPlayerID);

        for (int i = 0; i < hand.Count; i++)
        {
            Card card = hand[i];

            Card_Presenter prefab;

            if (isLocal)
            {
                prefab = cardPresenters.Find(cp =>
                    cp.card_SO.Name == card.GetName() &&
                    cp.card_SO.Suit == card.GetSuit());
            }
            else
            {
                prefab = cardBack;
            }

            Card_Presenter presenter = Instantiate(prefab, root);
            presenter.card = card;

            if (isLocal)
            {
                var click = presenter.gameObject.AddComponent<CardClickController>();
                click.cardIndex = i;
                click.playerID = playerID;
            }
        }
    }
}
