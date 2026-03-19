using System;
using System.Collections.Generic;
using UnityEngine;

public class Deck_Tester : MonoBehaviour
{
    Deck deck = new Deck();

    Player player1 = new Player(1);
    Player player2 = new Player(2);

    Card player1_card;
    Card player2_card;

    [SerializeField] int cards_played;

    enum PlayerOnTurn { player1, player2 }
    PlayerOnTurn player_on_turn;

    enum GameState { Preparation, Phase1, Phase2 }
    GameState game_state;

    bool is_round_finished;

    void Start()
    {
        //Debug.Log(deck.GetCards().Count);
        InitDeck();

        DetermineKoz();

        InitPlayersHand();

        player1.PrintHand();
        player2.PrintHand();

        PutKozAsLastCard();

        game_state = GameState.Phase1;
    }

    private void Update()
    {
        for (int i = 1; i <= 6; i++)
        {
            if (Input.GetKeyDown(i.ToString()))
            {
                HandleInput(i - 1);
            }
        }

        if (deck.GetCards().Count == 0)
        {
            game_state = GameState.Phase2;
        }

        if (!is_round_finished)
        {
            RoundWinner();
        }   
    }

    private void HandleInput(int input)
    {
        switch (player_on_turn)
        {
            case PlayerOnTurn.player1:
                player1_card = PlayerPlayCard(player1, input);
                player_on_turn = PlayerOnTurn.player2;
                cards_played++;
                break;
            case PlayerOnTurn.player2:
                player2_card = PlayerPlayCard(player2, input);
                player_on_turn = PlayerOnTurn.player1;
                cards_played++;
                break;
            default:
                break;
        }

        if (cards_played == 2)
        {
            switch (game_state)
            {
                case GameState.Preparation:
                    break;
                case GameState.Phase1:
                    DetermineWhoGetsPoints(player1_card, player2_card);
                    break;
                case GameState.Phase2:
                    break;
                default:
                    break;
            }

            
            Debug.Log($"Player on turn {player_on_turn}");
        }
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
            deck.CutDeck();
            deck.RoseShuffle();
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

    string DetermineKoz()
    {
        List<Card> temp_cards = new List<Card>();
        Queue<Card> temp_deck = new Queue<Card>(deck.GetCards());
        
        for (int i = 23; i >= 0; i--)
        {
            temp_cards.Add(temp_deck.Dequeue());
        }
        //THE KOZ IS ALWAYS THE 13th ELEMENT!
        Debug.Log($"KOZ IS: {temp_cards[12].GetName()} {temp_cards[12].GetSuit()}");

        foreach (Card card in deck.GetCards())
        {
            if (card.GetSuit() == temp_cards[12].GetSuit())
            {
                card.SetKoz(true);
                Debug.Log($"{card.GetName()} {card.GetSuit()} IS KOZ");
            }
        }

        return temp_cards[12].GetSuit();


        //leftover_deck = new Stack<Card>(deck.GetCards());
        //Debug.Log($"KOZ IS: {leftover_deck.Peek().GetSuit()}");
        //return leftover_deck.Pop().GetSuit();
    }

    void PutKozAsLastCard()
    { 
        List<Card> temp_list = new List<Card>();

        Card koz = deck.GetCards().Pop();

        Debug.Log($"KOZ IS SAME: {koz.GetName()} {koz.GetSuit()}");

        for (int i = 0; i < 11; i++)
        {
            temp_list.Add(deck.GetCards().Pop());
        }

        temp_list.Reverse();

        temp_list.Insert(0,koz);

        for (int i = 0; i < temp_list.Count; i++)
        { 
            deck.GetCards().Push(temp_list[i]);
        }
    }

    Card PlayerPlayCard(Player player, int index)
    {
        Card played_card = player.PlayCard(index);
        return played_card; 
    }

    void DetermineWhoGetsPoints(Card card1, Card card2)
    {
        if (card1.GetPoints() > card2.GetPoints() && card1.GetSuit() == card2.GetSuit())
        {
            player1.AddToRoundPoints(card1, card2);
            Debug.Log($"Player1 took the cards. Player1 plays first!");
            if (deck.GetCards().Count >= 2)
            {
                player1.TakeCardFromDeck(deck);
                player2.TakeCardFromDeck(deck);
            }
            player1.PrintHand();
            player2.PrintHand();
            player1.PrintPoints();
            player2.PrintPoints();
            player_on_turn = PlayerOnTurn.player1;
            cards_played = 0;
        }
        else if (player_on_turn == PlayerOnTurn.player1 && card1.GetSuit() != card2.GetSuit())
        {
            player1.AddToRoundPoints(card1, card2);
            Debug.Log($"Player1 took the cards. Player1 plays first!");
            if (deck.GetCards().Count >= 2)
            {
                player1.TakeCardFromDeck(deck);
                player2.TakeCardFromDeck(deck);
            }
            player1.PrintHand();
            player2.PrintHand();
            player1.PrintPoints();
            player2.PrintPoints();
            player_on_turn = PlayerOnTurn.player1;
            cards_played = 0;
        }
        else if (player_on_turn == PlayerOnTurn.player1 && card1.GetSuit() != card2.GetSuit() && card2.GetKoz() == true)
        {
            player2.AddToRoundPoints(card1, card2);
            Debug.Log($"Player1 took the cards. Player1 plays first!");
            if (deck.GetCards().Count >= 2)
            {
                player2.TakeCardFromDeck(deck);
                player1.TakeCardFromDeck(deck);
            }
            player1.PrintHand();
            player2.PrintHand();
            player1.PrintPoints();
            player2.PrintPoints();
            player_on_turn = PlayerOnTurn.player2;
            cards_played = 0;
        }





        else if (card1.GetPoints() < card2.GetPoints() && card1.GetSuit() == card2.GetSuit())
        {
            player2.AddToRoundPoints(card1, card2);
            Debug.Log($"Player2 took the cards. Player2 plays first!");
            if (deck.GetCards().Count >= 2)
            {
                player2.TakeCardFromDeck(deck);
                player1.TakeCardFromDeck(deck);
            }
            player1.PrintHand();
            player2.PrintHand();
            player1.PrintPoints();
            player2.PrintPoints();
            player_on_turn = PlayerOnTurn.player2;
            cards_played = 0;
        }
        else if (player_on_turn == PlayerOnTurn.player2 && card1.GetSuit() != card2.GetSuit())
        {
            player2.AddToRoundPoints(card1, card2);
            Debug.Log($"Player1 took the cards. Player1 plays first!");
            if (deck.GetCards().Count >= 2)
            {
                player2.TakeCardFromDeck(deck);
                player1.TakeCardFromDeck(deck);
            }
            player1.PrintHand();
            player2.PrintHand();
            player1.PrintPoints();
            player2.PrintPoints();
            player_on_turn = PlayerOnTurn.player2;
            cards_played = 0;
        }
        else if (player_on_turn == PlayerOnTurn.player2 && card1.GetSuit() != card2.GetSuit() && card1.GetKoz() == true)
        {
            player1.AddToRoundPoints(card1, card2);
            Debug.Log($"Player1 took the cards. Player1 plays first!");
            if (deck.GetCards().Count >= 2)
            {
                player1.TakeCardFromDeck(deck);
                player2.TakeCardFromDeck(deck);
            }
            player1.PrintHand();
            player2.PrintHand();
            player1.PrintPoints();
            player2.PrintPoints();
            player_on_turn = PlayerOnTurn.player1;
            cards_played = 0;
        }
    }

    Player RoundWinner()
    {
        if (player1.GetRoundPoints() >= 66)
        {
            if (player2.GetRoundPoints() == 0)
            {
                player1.AddToGamePoints(3);
            }
            else if (player2.GetRoundPoints() < 33)
            {
                player1.AddToGamePoints(2);
            }
            else
            {
                player1.AddToGamePoints(1);
            }
            is_round_finished = true;
            Debug.Log("Player1 WON!");
            return player1;
        }
        else if (player2.GetRoundPoints() >= 66)
        {

            if (player1.GetRoundPoints() == 0)
            {
                player2.AddToGamePoints(3);
            }
            else if (player1.GetRoundPoints() < 33)
            {
                player2.AddToGamePoints(2);
            }
            else
            {
                player2.AddToGamePoints(1);
            }
            is_round_finished = true;
            Debug.Log("Player2 WON!");
            return player2;
        }

        return null;
    }
}
