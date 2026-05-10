using System.Collections.Generic;
using UnityEngine;

public class Player
{
    public int ID { get; private set; }

    List<Card> hand = new List<Card>();
    int roundPoints;
    int gamePoints;

    public Player()
    {
            
    }
    public Player(int ID)
    {
        this.ID = ID;
    }

    public Card TakeCardFromDeck(Deck deck)
    {
        Card card = deck.GetCards().Pop();
        hand.Add(card);

        return card;
    }

    public List<Card> GetHand()
    { 
        return hand;
    }

    public Card PlayCard(int index)
    {
        Debug.Log($"Player {ID} played card {hand[index].GetName()} of {hand[index].GetSuit()}");
        Card removed = hand[index];
        CheckFor20or40(removed);

        return removed;
    }

    private void CheckFor20or40(Card removed)
    {
        if (removed.GetName() == "Q")
        {
            foreach (Card card in hand)
            {
                if (card.GetName() == "K" && card.GetSuit() == removed.GetSuit())
                {
                    if (removed.GetKoz() == true)
                    {
                        roundPoints += 40;
                    }
                    else
                    {
                        roundPoints += 20;
                    }
                }
            }
        }
    }

    public void AddToRoundPoints(Card card1, Card card2)
    {
        roundPoints += card1.GetPoints() + card2.GetPoints();
    }
    public void AddToGamePoints(byte points)
    {
        gamePoints += points;
    }

    public int GetRoundPoints()
    { 
        return roundPoints;
    }

    public int GetGamePoints()
    { 
        return gamePoints;
    }

    public void PrintHand()
    {
        string cards = "";
        for (int i = 0; i < hand.Count; i++)
        {
            cards += $"[{i+1}]" + "("+ hand[i].GetName().ToString() + " " + hand[i].GetSuit().ToString() + ") ";
        }
        Debug.Log($"Player{ID} {cards}");
    }

    public void PrintPoints()
    {
        Debug.Log($"Player{ID} points: {GetRoundPoints()}");
    }

    //KOZ == TRUMP CARD (in english)
    public Card Change9Koz(Deck deck)
    {
        Card card_9 = new Card();
        for (int i = 0; i < hand.Count; i++)
        {
            if (hand[i].GetName() == "9" && hand[i].GetKoz())
            {
                card_9 = hand[i];
            }
        }
       

        if (deck.GetCards().Count > 2 && card_9.GetName() == "9")
        {
            hand.Remove(card_9);
            hand.Add(deck.GetCards().ToArray()[deck.GetCards().Count - 1]);
            Card exchanged = deck.GetCards().ToArray()[deck.GetCards().Count - 1];
            deck.GetCards().ToArray()[deck.GetCards().Count - 1] = card_9;

            Debug.Log($"Player{ID} exchanged {card_9.GetName()} of {card_9.GetSuit()} and got {exchanged.GetName()} of {exchanged.GetSuit()}");
            return card_9;  
        }
        else
        {
            Debug.Log("Can't exchange!");
            return null;
        }
    }

    public void ResetRoundPoints()
    {
        roundPoints = 0;
    }

    public void ResetGamePoints()
    {
        gamePoints = 0;
    }

    public void ClearHand()
    {
        hand.Clear();
    }
}
