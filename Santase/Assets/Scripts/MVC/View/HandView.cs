using System.Collections.Generic;
using UnityEngine;
public class HandView : MonoBehaviour
{
    [SerializeField] Transform player1HandRoot;
    [SerializeField] Transform player2HandRoot;
    [SerializeField] List<CardPresenter> cardPresenters;
    [SerializeField] private CardPresenter cardBack;

    public void UpdateHand(int playerID, List<Card> hand)
    {
        Transform root = GetRootForPlayer(playerID);


        foreach (Transform child in root)
            Destroy(child.gameObject);

        bool isLocal = (playerID == GameController.Instance.localPlayerID);

        for (int i = 0; i < hand.Count; i++)
        {
            Card card = hand[i];

            CardPresenter prefab;

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

            CardPresenter presenter = Instantiate(prefab, root);
            presenter.card = card;

            if (isLocal)
            {
                var click = presenter.gameObject.AddComponent<CardClickController>();
                click.cardIndex = i;
                click.playerID = playerID;
            }
        }
    }
    private Transform GetRootForPlayer(int playerID)
    {
        if (GameController.Instance.localPlayerID == 0)  return playerID == 0 ? player1HandRoot : player2HandRoot;

        else return playerID == 1 ? player1HandRoot : player2HandRoot;
    }
}
