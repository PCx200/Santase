using UnityEngine;
using System.Collections.Generic;

public class Card
{
    string name;
    string suit;
    int points;

    bool is_koz;

    public Card()
    {
            
    }
    public Card(string name, string suit, int points)
    {
        this.name = name;
        this.suit = suit;
        this.points = points;
    }

    public Card(string name, string suit, bool is_koz)
    {
        this.name = name;
        this.suit = suit;
        this.is_koz = is_koz;
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

    public void SetKoz(bool koz)
    {
        is_koz = koz;
    }

    public bool GetKoz()
    {
        return is_koz;
    }
}
