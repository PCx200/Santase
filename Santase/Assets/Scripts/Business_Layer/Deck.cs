using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public class Deck
{
    string card_data_path = "C:\\Users\\ACER\\Documents\\Santase\\Santase\\Assets\\Scripts\\Data_Layer\\Cards.txt";
    Stack<Card> cards = new Stack<Card>();

    public Deck()
    {
        FillDeckFromFile();
    }

    void FillDeckFromFile()
    {
        foreach (var line in File.ReadLines(card_data_path))
        {
            if (string.IsNullOrWhiteSpace(line))
                continue;

            string[] parts = line.Split(' ');

            string name = parts[0];
            string suit = parts[1];
            int points = int.Parse(parts[2]);

            Card c = new Card(name, suit, points);

            cards.Push(c);
        }
    }

    public Stack<Card> RandomShuffle()
    {
        List<Card> temp_deck = new List<Card>();
        for (int i = 0; i < 24; i++)
        {
            temp_deck.Add(cards.Pop());
        }

        List<Card> shuffled_deck = new List<Card>();

        for (int i = 0; i < 24; i++)
        {
            System.Random random = new System.Random();
            int card_index = random.Next(temp_deck.Count);
            shuffled_deck.Add(temp_deck[card_index]);
            temp_deck.Remove(temp_deck[card_index]);
        }

        for (int i = 0; i < shuffled_deck.Count; i++)
        {
            cards.Push(shuffled_deck[i]);
        }

        return cards;
    }

    public Stack<Card> SplitDeck()
    { 
        Stack<Card> temp_list1 = new Stack<Card>();
        Stack<Card> temp_list2 = new Stack<Card>();

        System.Random rand = new System.Random();

        int cards_lifted = rand.Next(3, 22);
        int cards_left = cards.Count - cards_lifted;

        Debug.Log($"Cards lifted: {cards_lifted}.");

        for (int i = 0; i < cards_left; i++)
        {
            temp_list2.Push(cards.Pop());
        }
        for (int i = 0; i < cards_lifted; i++)
        {
            temp_list1.Push(cards.Pop());
        }

        Debug.Log($"Temp1 Count : {temp_list1.Count}");
        Debug.Log($"Temp2 Count : {temp_list2.Count}");

        for (int i = 0; i < cards_left; i++)
        {
            cards.Push(temp_list2.Pop());
        }
        for (int i = 0; i < cards_lifted; i++)
        {
            cards.Push(temp_list1.Pop());
        }


        return cards;
    }

    public Stack<Card> GetCards()
    { 
        return cards;
    }
}
