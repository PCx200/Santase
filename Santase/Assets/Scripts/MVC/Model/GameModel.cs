using System;
using System.Collections.Generic;
using UnityEngine;

public class GameModel
{

    public event Action<int, List<Card>> OnHandChanged;
    public event Action<Card> OnKozChanged;
    public event Action<int, int> OnScoreChanged;
    public event Action<int> OnTurnChanged;
    public event Action<Card, int> OnCardPlayed;
    public event Action<Player> OnRoundOver;
    public event Action<string> OnNotification;
    public event Action<Player> OnMatchOver;

    public enum GameState { Preparation, Phase1, Phase2 }

    public Player player1 = new Player(0);
    public Player player2 = new Player(1);

    public Deck deck = new Deck();

    public GameState gameState = GameState.Preparation;

    private int activePlayer = 0;
    private int cardsPlayed = 0;

    private Card p1Played;
    private Card p2Played;

    private Card kozCard;


    public GameModel()
    {
        InitDeck();
        DetermineKoz();
        PutKozAsLastCard();
        DealInitialHands();

        gameState = GameState.Phase1;

        activePlayer = 0;
        OnTurnChanged?.Invoke(activePlayer);
    }

    #region Initialization
    private void InitDeck()
    {
        for (int i = 0; i < 20; i++)
        {
            deck.RandomShuffle();
            deck.CutDeck();
            deck.RoseShuffle();
        }
    }

    private void DealInitialHands()
    {
        for (int i = 0; i < 3; i++) player1.TakeCardFromDeck(deck);
        for (int i = 0; i < 3; i++) player2.TakeCardFromDeck(deck);
        for (int i = 0; i < 3; i++) player1.TakeCardFromDeck(deck);
        for (int i = 0; i < 3; i++) player2.TakeCardFromDeck(deck);
    }

    private void DetermineKoz()
    {
        List<Card> temp = new List<Card>(deck.GetCards());
        temp.Reverse();

        kozCard = temp[11];
        kozCard.SetKoz(true);

        foreach (Card c in deck.GetCards())
        {
            if (c.GetSuit() == kozCard.GetSuit())
                c.SetKoz(true);
        }

        Debug.Log(kozCard.GetName());
    }

    private void PutKozAsLastCard()
    {
        deck.SetLast(kozCard);
    }
    #endregion

    #region Gameplay Logic
    public void RequestPlayCard(int playerID, int cardIndex)
    {
        if (playerID != activePlayer)
        {
            OnNotification?.Invoke("Not your turn.");
            return;
        }

        Player player = playerID == 0 ? player1 : player2;

        if (cardIndex < 0 || cardIndex >= player.GetHand().Count)
        {
            OnNotification?.Invoke("Invalid card.");
            return;
        }

        Card played = player.PlayCard(cardIndex);

        if (playerID == 0)
            p1Played = played;
        else
            p2Played = played;

        OnCardPlayed?.Invoke(played, playerID);

        cardsPlayed++;

        if (cardsPlayed == 2)
        {
            ResolveHand();
            return;
        }

        activePlayer = 1 - activePlayer;
        OnTurnChanged?.Invoke(activePlayer);
    }

    public void RequestExchangeKoz(int playerID)
    {
        if (playerID != activePlayer)
        {
            OnNotification?.Invoke("Not your turn.");
            return;
        }

        if (deck.GetCards().Count <= 2)
        {
            OnNotification?.Invoke("Cannot exchange in Phase 2.");
            return;
        }

        Player player = playerID == 0 ? player1 : player2;

        Card exchanged = player.Change9Koz(deck);
        PutKozAsLastCard();

        if (exchanged == null)
        {
            OnNotification?.Invoke("You cannot exchange the koz card.");
            return;
        }

        OnHandChanged?.Invoke(playerID, player.GetHand());
        OnKozChanged?.Invoke(exchanged);
    }

    private void ResolveHand()
    {
        // PHASE 2 INVALID MOVE CHECK
        if (gameState == GameState.Phase2)
        {
            Player leader = activePlayer == 0 ? player1 : player2;
            Player follower = activePlayer == 0 ? player2 : player1;

            Card leaderCard = activePlayer == 0 ? p1Played : p2Played;
            Card followerCard = activePlayer == 0 ? p2Played : p1Played;

            bool followerHasSuit = follower.GetHand().Exists(c => c.GetSuit() == leaderCard.GetSuit());

            if (followerHasSuit && followerCard.GetSuit() != leaderCard.GetSuit())
            {
                ReturnPlayedCardsToHands();
                cardsPlayed = 0;

                OnNotification?.Invoke("You must follow suit if possible.");
                OnHandChanged?.Invoke(0, player1.GetHand());
                OnHandChanged?.Invoke(1, player2.GetHand());
                return;
            }
        }

        Player winner = null;
        Player loser = null;

        Card c1 = p1Played;
        Card c2 = p2Played;


        if (c1.GetPoints() > c2.GetPoints() && c1.GetSuit() == c2.GetSuit())
        {
            winner = player1; loser = player2;
        }

        else if (activePlayer == player1.ID && c1.GetSuit() != c2.GetSuit() && !c2.GetKoz())
        {
            winner = player1; loser = player2;
        }

        else if (activePlayer == player1.ID && c1.GetSuit() != c2.GetSuit() && c2.GetKoz())
        {
            winner = player2; loser = player1;
        }

        else if (c1.GetPoints() < c2.GetPoints() && c1.GetSuit() == c2.GetSuit())
        {
            winner = player2; loser = player1;
        }

        else if (activePlayer == player2.ID && c1.GetSuit() != c2.GetSuit() && !c1.GetKoz())
        {
            winner = player2; loser = player1;
        }

        else if (activePlayer == player2.ID && c1.GetSuit() != c2.GetSuit() && c1.GetKoz())
        {
            winner = player1; loser = player2;
        }


        winner.AddToRoundPoints(c1, c2);


        player1.GetHand().Remove(c1);
        player2.GetHand().Remove(c2);

        OnHandChanged?.Invoke(0, player1.GetHand());
        OnHandChanged?.Invoke(1, player2.GetHand());

        OnScoreChanged?.Invoke(player1.GetRoundPoints(), player2.GetRoundPoints());


        if (gameState == GameState.Phase1 && deck.GetCards().Count >= 2)
        {
            winner.TakeCardFromDeck(deck);
            loser.TakeCardFromDeck(deck);

            OnHandChanged?.Invoke(0, player1.GetHand());
            OnHandChanged?.Invoke(1, player2.GetHand());
        }


        if (deck.IsEmpty())
            gameState = GameState.Phase2;


        Player roundWinner = DetermineRoundWinner();
        if (roundWinner != null)
        {
            OnRoundOver?.Invoke(roundWinner);
            return;
        }


        activePlayer = winner.ID;
        cardsPlayed = 0;
        OnTurnChanged?.Invoke(activePlayer);
    }

    private void ReturnPlayedCardsToHands()
    {
        if (p1Played != null) player1.GetHand().Add(p1Played);
        if (p2Played != null) player2.GetHand().Add(p2Played);

        p1Played = null;
        p2Played = null;
    }


    private Player DetermineRoundWinner()
    {
        Player winner = null;

        if (player1.GetRoundPoints() >= 66)
        {
            if (player2.GetRoundPoints() == 0) player1.AddToGamePoints(3);
            else if (player2.GetRoundPoints() < 33) player1.AddToGamePoints(2);
            else player1.AddToGamePoints(1);

            winner = player1;
        }

        if (player2.GetRoundPoints() >= 66)
        {
            if (player1.GetRoundPoints() == 0) player2.AddToGamePoints(3);
            else if (player1.GetRoundPoints() < 33) player2.AddToGamePoints(2);
            else player2.AddToGamePoints(1);

            winner = player2;
        }

        if (winner != null && winner.GetGamePoints() >= 11)
        {
            OnMatchOver?.Invoke(winner);
            return winner;
        }

        return winner;
    }

    public void RestartRound()
    {
        player1.ResetRoundPoints();
        player2.ResetRoundPoints();

        player1.ClearHand();
        player2.ClearHand();

        deck = new Deck();
        InitDeck();
        DetermineKoz();
        PutKozAsLastCard();
        DealInitialHands();

        activePlayer = 0;
        cardsPlayed = 0;
        gameState = GameState.Phase1;

        OnHandChanged?.Invoke(0, player1.GetHand());
        OnHandChanged?.Invoke(1, player2.GetHand());
        OnKozChanged?.Invoke(kozCard);
        OnScoreChanged?.Invoke(player1.GetRoundPoints(), player2.GetRoundPoints());
        OnTurnChanged?.Invoke(activePlayer);
    }
    public void RestartMatch()
    {
        player1.ResetGamePoints();
        player2.ResetGamePoints();
        RestartRound();
    }
    #endregion

    #region Helper Methods
    public int GetActivePlayer()
    {
        return activePlayer;
    }
    public void ForceFullUpdate()
    {
        OnHandChanged?.Invoke(0, player1.GetHand());
        OnHandChanged?.Invoke(1, player2.GetHand());
        OnKozChanged?.Invoke(kozCard);
        OnScoreChanged?.Invoke(player1.GetRoundPoints(), player2.GetRoundPoints());
        OnTurnChanged?.Invoke(activePlayer);
    }

    #endregion
}
