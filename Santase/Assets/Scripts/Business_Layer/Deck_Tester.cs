using JetBrains.Annotations;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Deck_Tester : MonoBehaviour
{
    Deck deck = new Deck();

    static Player player1 = new Player(0);
    static Player player2 = new Player(1);

    Card player1_card;
    Card player2_card;

    [SerializeField] int cards_played;

    public List<Card_Presenter> card_presenters;
    int player_on_turn = player1.ID;

    public enum GameState { Preparation, Phase1, Phase2 }
    [SerializeField] GameState game_state;

    bool is_round_finished;

    [SerializeField] Transform player1_hand;
    [SerializeField] Transform player2_hand;
    [SerializeField] Transform koz_transform;

    [SerializeField] Transform player1_card_spot;
    [SerializeField] Transform player2_card_spot;

    [SerializeField] TextMeshProUGUI player1_score;
    [SerializeField] TextMeshProUGUI player2_score;

    private void Start()
    {
        var sw = new System.Diagnostics.Stopwatch();
        sw.Start();
        InitDeck();

        DetermineKoz();

        InitPlayersHand();

        player1.PrintHand();
        player2.PrintHand();

        VisualizeHand(player1, player1_hand);
        VisualizeHand(player2, player2_hand);

        Card koz_card = PutKozAsLastCard();
        VisualizeKoz(koz_card, koz_transform);


        game_state = GameState.Phase1;
        sw.Stop();

        Debug.Log(sw.ElapsedMilliseconds + " Milliseconds to execute the shuffling ");
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

        if (Input.GetKeyDown(KeyCode.Space))
        {
            Card card_9 = new Card();

            card_9 = player1.Change9Koz(deck);

            if (card_9 == null)
            {
                card_9 = player2.Change9Koz(deck);
                deck.SetLast(card_9);

                VisualizeKoz(card_9, koz_transform);
                VisualizeHand(player2, player2_hand);
            }
            else
            {
                deck.SetLast(card_9);
 
                VisualizeKoz(card_9, koz_transform);
                VisualizeHand(player1, player1_hand);
            }
            
            
        }
    }

    private void HandleInput(int input)
    {
        switch (player_on_turn)
        {
            case 0: // Player1 Turn
                player1_card = PlayerPlayCard(player1, input);

                player1.GetHand().Remove(player1_card);
                VisualizeHand(player1, player1_hand);

                VisualizePlayedCard(player1_card, player1_card_spot);
                player_on_turn = player2.ID;
                cards_played++;
                break;
            case 1: // Player2 Turn
                player2_card = PlayerPlayCard(player2, input);

                player1.GetHand().Remove(player1_card);
                VisualizeHand(player2, player2_hand);

                VisualizePlayedCard(player2_card, player2_card_spot);
                player_on_turn = player1.ID;
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
                    player1.GetHand().Remove(player1_card);
                    player2.GetHand().Remove(player2_card);
                    VisualizeHand(player1, player1_hand);
                    VisualizeHand(player2, player2_hand);
                    break;
                case GameState.Phase2: // TODO:: FIX THIS PHASE
                    if (player1_card.GetSuit() == player2_card.GetSuit())
                    {
                        DetermineWhoGetsPoints(player1_card, player2_card);
                        player1.GetHand().Remove(player1_card);
                        player2.GetHand().Remove(player2_card);
                        VisualizeHand(player1, player1_hand);
                        VisualizeHand(player2, player2_hand);
                    }
                    else {
                        Debug.Log("You must play the same suit as the opponent if you have it.");
                        cards_played = 0;
                    }

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

    private void VisualizeHand(Player player, Transform hand_transform)
    {
        foreach (Transform child in hand_transform)
        { 
            Destroy(child.gameObject);
        }

        foreach (Card card in player.GetHand())
        {
            Card_Presenter prefab = card_presenters.Find(cp =>
            cp.card_SO.Name == card.GetName() &&
            cp.card_SO.Suit == card.GetSuit()
            );

            Card_Presenter cp = Instantiate(prefab, hand_transform);
            cp.card = card;

        }
    }

    private void VisualizeKoz(Card koz_card, Transform koz_transform)
    {
        Card_Presenter prefab = card_presenters.Find(cp =>
            cp.card_SO.Name == koz_card.GetName() &&
            cp.card_SO.Suit == koz_card.GetSuit()
            );

        Card_Presenter cp = Instantiate(prefab, koz_transform);
        cp.card = koz_card;
    }

    private void VisualizePlayedCard(Card played_card, Transform played_card_transform)
    { 
        Card_Presenter prefab = card_presenters.Find(cp =>
            cp.card_SO.Name == played_card.GetName() &&
            cp.card_SO.Suit == played_card.GetSuit()
            );

        played_card_transform.GetComponent<Image>().sprite = prefab.card_SO.sprite;
        Color color = played_card_transform.GetComponent<Image>().color;
        color.a = 1;
        played_card_transform.GetComponent<Image>().color = color;
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
    }

    private string DetermineKoz()
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
    }

    private Card PutKozAsLastCard()
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

        return koz;
    }

    private Card PlayerPlayCard(Player player, int index)
    {
        Card played_card = player.PlayCard(index);
        return played_card; 
    }

    private void PrintPlayersHandsAndPoints()
    {
        player1.PrintHand();
        player2.PrintHand();
        player1.PrintPoints();
        player2.PrintPoints();
    }

    private void HandleHandWinner(Player winner, Player loser, Card player1_card, Card player2_card)
    {
        winner.AddToRoundPoints(player1_card, player2_card);
        Debug.Log($"{winner} took the cards. {winner} plays first!");
        if (deck.GetCards().Count >= 2)
        {
            winner.TakeCardFromDeck(deck);
            loser.TakeCardFromDeck(deck);
        }

        player_on_turn = player1 == winner ? player1.ID : player2.ID;

        cards_played = 0;
    }

    private Player RoundWinner()
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

    private void DetermineWhoGetsPoints(Card player1_card, Card player2_card)
    {
        if (player1_card.GetPoints() > player2_card.GetPoints() && player1_card.GetSuit() == player2_card.GetSuit())
        {
            HandleHandWinner(player1, player2, player1_card, player2_card);

            PrintPlayersHandsAndPoints();

            if (deck.IsEmpty())
            {
                game_state = GameState.Phase2;
            }
        }
        else if (player_on_turn == player1.ID && player1_card.GetSuit() != player2_card.GetSuit() && !player2_card.GetKoz())
        {
            HandleHandWinner(player1, player2, player1_card, player2_card);

            PrintPlayersHandsAndPoints();

            if (deck.IsEmpty())
            {
                game_state = GameState.Phase2;
            }
        }
        else if (player_on_turn == player1.ID && player1_card.GetSuit() != player2_card.GetSuit() && player2_card.GetKoz())
        {
            HandleHandWinner(player2, player1, player1_card, player2_card);

            PrintPlayersHandsAndPoints();

            if (deck.IsEmpty())
            {
                game_state = GameState.Phase2;
            }
        }




        else if (player1_card.GetPoints() < player2_card.GetPoints() && player1_card.GetSuit() == player2_card.GetSuit())
        {
            HandleHandWinner(player2, player1, player1_card, player2_card);

            PrintPlayersHandsAndPoints();

            if (deck.IsEmpty())
            {
                game_state = GameState.Phase2;
            }
        }
        else if (player_on_turn == player2.ID && player1_card.GetSuit() != player2_card.GetSuit() && !player1_card.GetKoz())
        {
            HandleHandWinner(player2, player1, player1_card, player2_card);

            PrintPlayersHandsAndPoints();

            if (deck.IsEmpty())
            {
                game_state = GameState.Phase2;
            }
        }
        else if (player_on_turn == player2.ID && player1_card.GetSuit() != player2_card.GetSuit() && player1_card.GetKoz())
        {
            HandleHandWinner(player1, player2, player1_card, player2_card);

            PrintPlayersHandsAndPoints();

            if (deck.IsEmpty())
            {
                game_state = GameState.Phase2;
            }
        }

        player1_score.text = player1.GetRoundPoints().ToString();
        player2_score.text = player2.GetRoundPoints().ToString();

        RoundWinner();
    }
}
