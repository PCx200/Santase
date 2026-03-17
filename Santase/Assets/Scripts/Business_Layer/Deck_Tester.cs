using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Deck_Tester : MonoBehaviour
{
    Deck deck = new Deck();
    [SerializeField] int size;
    void Start()
    {
        //Debug.Log(deck.GetCards().Count);
        int deck_size = deck.GetCards().Count;
        size = deck_size;
        for (int i = 0; i < 100; i++)
        {
            deck.RandomShuffle();
        }
        Stack<Card> temp_deck = new Stack<Card>(deck.GetCards());
        for (int i = 0; i < deck_size; i++)
        {
            Card card = temp_deck.Pop();
            Debug.Log($"{card.GetName()} {card.GetSuit()} {card.GetPoints()}");
        }
        Debug.Log($"Before split: {deck.GetCards().Count}");
        deck.SplitDeck();
        
        temp_deck = new Stack<Card>(deck.GetCards());
        Debug.Log($"After split: {deck.GetCards().Count}");
        for (int i = 0; i < deck_size; i++)
        {

            Card card = temp_deck.Pop();
            Debug.Log($"{card.GetName()} {card.GetSuit()} {card.GetPoints()}");
        }
    }
}
