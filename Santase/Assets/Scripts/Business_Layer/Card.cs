using UnityEngine;
using System.Collections.Generic;

public class Card
{
    string name;
    string suit;
    int points;

    public Card(string name, string suit, int points)
    {
        this.name = name;
        this.suit = suit;
        this.points = points;
    }

    public string GetName()
    { 
        return name;
    }
    public string GetSuit()
    {
        return suit;
    }
    public int GetPoints()
    {
        return points;
    }
}
