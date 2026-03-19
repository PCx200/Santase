using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class Player
{
    int id;

    List<Card> hand = new List<Card>();
    int round_points;
    int game_points;

    public Player()
    {
            
    }
    public Player(int id)
    {
        this.id = id;
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
        Debug.Log($"Player {id} played card {hand[index].GetName()} of {hand[index].GetSuit()}");
        Card removed = hand[index];
        hand.RemoveAt(index);
        CheckFor20or40(removed);

        return removed;
    }

    void CheckFor20or40(Card removed)
    {
        if (removed.GetName() == "Q")
        {
            foreach (Card card in hand)
            {
                if (card.GetName() == "K" && card.GetSuit() == removed.GetSuit())
                {
                    if (removed.GetKoz() == true)
                    {
                        round_points += 40;
                    }
                    else
                    {
                        round_points += 20;
                    }
                }
            }
        }
    }

    public void AddToRoundPoints(Card card1, Card card2)
    {
        round_points += card1.GetPoints() + card2.GetPoints();
    }
    public void AddToGamePoints(byte points)
    {
        game_points += points;
    }

    public int GetRoundPoints()
    { 
        return round_points;
    }

    public int GetGamePoints()
    { 
        return game_points;
    }

    public void PrintHand()
    {
        string cards = "";
        for (int i = 0; i < hand.Count; i++)
        {
            cards += $"[{i+1}]" + "("+ hand[i].GetName().ToString() + " " + hand[i].GetSuit().ToString() + ") ";
        }
        Debug.Log($"Player{id} {cards}");
    }

    public void PrintPoints()
    {
        Debug.Log($"Player{id} points: {GetRoundPoints()}");
    }

    //TODO::
    //public Card Change9Koz()
    //{ 
        
    //}
}
