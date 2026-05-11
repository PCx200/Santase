using System.Runtime.CompilerServices;
using UnityEngine;

public class DeckView : MonoBehaviour
{
    [SerializeField] GameObject deck;

    [SerializeField] GameObject closedDeck;

    public void EnableDeckView(bool enable)
    {
        deck.SetActive(enable);
    }

    public void CloseDeck(bool close)
    {
        closedDeck.SetActive(close);
    }
}
