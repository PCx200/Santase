using System.Collections.Generic;
using UnityEngine;

public class Player
{
    int id;

    List<Card> hand = new List<Card>();
    int round_points;
    int game_points;

    public Player(int id)
    {
        this.id = id;
    }

    public void TakeCardFromDeck(Deck deck)
    {
        hand.Add(deck.GetCards().Pop());
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
        return removed;
    }

    public void AddPoints(Card card1, Card card2)
    {
        round_points += card1.GetPoints() + card2.GetPoints();
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
            cards += $"[{i}]" + "("+ hand[i].GetName().ToString() + " " + hand[i].GetSuit().ToString() + ") ";
        }
        Debug.Log($"Player{id} {cards}");
    }

    public void PrintPoints()
    {
        Debug.Log($"Player{id} points: {GetRoundPoints()}");
    }

    //public Card Change9Koz()
    //{ 
        
    //}
}
