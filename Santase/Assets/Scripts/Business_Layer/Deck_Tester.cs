using System.Collections.Generic;
using UnityEngine;

public class Deck_Tester : MonoBehaviour
{
    Deck deck = new Deck();

    Player player1 = new Player(1);
    Player player2 = new Player(2);

    void Start()
    {
        //Debug.Log(deck.GetCards().Count);
        InitDeck();
        InitPlayersHand();

        //player1.PrintHand();
        //player2.PrintHand();

        DetermineKoz(deck.GetCards());
        Debug.Log(deck.GetCards().Count);

        Card cardPlayer1 = player1.PlayCard(3);
        Card cardPlayer2 = new Card();
        for (int i = 0; i < player2.GetHand().Count; i++)
        {
            if (cardPlayer1.GetSuit() == player2.GetHand()[i].GetSuit())
            {
                cardPlayer2 = player2.PlayCard(i);
                break;
            }
            if (i == player2.GetHand().Count - 1 && cardPlayer1.GetSuit() != player2.GetHand()[i].GetSuit())
            {
                Debug.Log("NO MATCHING CARD");
                cardPlayer2 = player2.PlayCard(i);
            }
        }

        if (cardPlayer1.GetPoints() > cardPlayer2.GetPoints())
        {
            player1.AddPoints(cardPlayer1, cardPlayer2);
            player1.PrintPoints();
            player2.PrintPoints();
        }
        else
        {
            player2.AddPoints(cardPlayer1, cardPlayer2);
            player1.PrintPoints();
            player2.PrintPoints();

        }

        player1.TakeCardFromDeck(deck);
        player2.TakeCardFromDeck(deck);

        //player1.PrintHand();
        //player2.PrintHand();

    }

    private void InitPlayersHand()
    {
        for (int i = 0; i < 3; i++)
        {
            player1.TakeCardFromDeck(deck);
        }
        for (int i = 0; i < 3; i++)
        {
            player2.TakeCardFromDeck(deck);
        }
        for (int i = 0; i < 3; i++)
        {
            player1.TakeCardFromDeck(deck);
        }
        for (int i = 0; i < 3; i++)
        {
            player2.TakeCardFromDeck(deck);
        }
    }

    private void InitDeck()
    {
        for (int i = 0; i < 20; i++)
        {
            deck.RandomShuffle();
            deck.RoseShuffle();
            deck.CutDeck();
        }

        deck.PrintDeck();

        //Stack<Card> temp_deck = new Stack<Card>(deck.GetCards());
        //for (int i = 0; i < deck.size; i++)
        //{
        //    Card card = temp_deck.Pop();
        //    Debug.Log($"{card.GetName()} {card.GetSuit()} {card.GetPoints()}");
        //}

        //deck.CutDeck();

        //temp_deck = new Stack<Card>(deck.GetCards());
        //for (int i = 0; i < deck.size; i++)
        //{
        //    Card card = temp_deck.Pop();
        //    Debug.Log($"{card.GetName()} {card.GetSuit()} {card.GetPoints()}");
        //}
    }

    string DetermineKoz(Stack<Card> leftover_deck)
    {
        leftover_deck = new Stack<Card>(deck.GetCards());
        Debug.Log($"KOZ IS: {leftover_deck.Pop().GetSuit()}");
        return leftover_deck.Pop().GetSuit();
    }
}
