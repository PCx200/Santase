using UnityEngine;
using System.Collections.Generic;
using System.IO;

public class Deck
{
    string card_data_path = "C:\\Users\\ACER\\Documents\\Santase\\Santase\\Assets\\Scripts\\Data_Layer\\Cards.txt";
    Stack<Card> cards = new Stack<Card>();
    public const int size = 24;

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
        for (int i = 0; i < size; i++)
        {
            temp_deck.Add(cards.Pop());
        }

        List<Card> shuffled_deck = new List<Card>();

        for (int i = 0; i < size; i++)
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

    public Stack<Card> CutDeck()
    { 
        Stack<Card> temp_stack1 = new Stack<Card>();
        Stack<Card> temp_stack2 = new Stack<Card>();

        System.Random rand = new System.Random();

        int cards_lifted = rand.Next(3, 22);
        int cards_left = cards.Count - cards_lifted;

        //Debug.Log($"Cards lifted: {cards_lifted}.");

        for (int i = 0; i < cards_lifted; i++)
        {
            temp_stack1.Push(cards.Pop());
        }
        for (int i = 0; i < cards_left; i++)
        {
            temp_stack2.Push(cards.Pop());
        }


        //Debug.Log($"Temp1 Count : {temp_stack1.Count}");
        //Debug.Log($"Temp2 Count : {temp_stack2.Count}");

        for (int i = 0; i < cards_lifted; i++)
        {
            cards.Push(temp_stack1.Pop());
        }
        for (int i = 0; i < cards_left; i++)
        {
            cards.Push(temp_stack2.Pop());
        }

        return cards;
    }

    public Stack<Card> RoseShuffle()
    {
        List<Card> temp_pile1 = new List<Card>();
        List<Card> temp_pile2 = new List<Card>();

        System.Random random = new System.Random();
        int pile1_cards = random.Next(10, 14);
        int pile2_cards = size - pile1_cards; 

        for (int i = 0; i < pile1_cards; i++)
        {
            temp_pile1.Add(cards.Pop());
        }

        for (int i = 0; i < pile2_cards; i++)
        {
            temp_pile2.Add(cards.Pop());
        }

        if (pile1_cards < pile2_cards)
        {
            for (int i = pile1_cards - 1; i >= 0; i--)
            {
                cards.Push(temp_pile1[i]);
                cards.Push(temp_pile2[i]);
                temp_pile1.RemoveAt(i);
                temp_pile2.RemoveAt(i);
            }
            for (int i = (pile2_cards - pile1_cards) - 1; i >= 0; i--)
            {
                cards.Push(temp_pile2[i]);
                temp_pile2.RemoveAt(i);
            }
        }
        else if (pile1_cards > pile2_cards)
        {
            for (int i = pile2_cards - 1; i >= 0; i--)
            {
                cards.Push(temp_pile1[i]);
                cards.Push(temp_pile2[i]);
                temp_pile1.RemoveAt(i);
                temp_pile2.RemoveAt(i);
            }
            for (int i = (pile1_cards - pile2_cards) - 1; i >= 0; i--)
            {
                cards.Push(temp_pile1[i]);
                temp_pile1.RemoveAt(i);
            }
        }
        else
        {
            for (int i = size/2 - 1; i >= 0; i--)
            {
                cards.Push(temp_pile1[i]);
                cards.Push(temp_pile2[i]);
                temp_pile1.RemoveAt(i);
                temp_pile2.RemoveAt(i);
            }
        }
        return cards;
    }

    public Stack<Card> GetCards()
    { 
        return cards;
    }

    public void PrintDeck()
    {
        Stack<Card> temp_deck = new Stack<Card>(cards);
        for (int i = 0; i < size; i++)
        {
            Card card = temp_deck.Pop();
            Debug.Log($"{card.GetName()} {card.GetSuit()} {card.GetPoints()}");
        }
    }

    public bool IsEmpty()
    {
        if (cards.Count == 0)
        {
            return true;
        }
        return false;
    }
}
