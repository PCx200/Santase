using UnityEngine;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System;

public class GameController : MonoBehaviour
{
    GameModel model;

    [Header("Multiplayer")]
    public int localPlayerID = -1;

    [Header("Views")]
    [SerializeField] HandView handView;
    [SerializeField] KozView kozView;
    [SerializeField] PlayedCardView playedCardView;
    [SerializeField] ScoreView scoreView;
    [SerializeField] TurnView turnView;
    [SerializeField] NotificationView notificationView;
    [SerializeField] RoundOverView roundOverView;
    [SerializeField] DeckView deckView;

    private void Start()
    {
        if (localPlayerID == -1)
            localPlayerID = 0;

        handView.localPlayerID = localPlayerID;

        System.Random rand = new System.Random();
        int seed = rand.Next(int.MinValue, int.MaxValue);
        model = new GameModel(seed);

        model.OnHandChanged += HandleHandChanged;
        model.OnKozChanged += HandleKozChanged;
        model.OnScoreChanged += HandleScoreChanged;
        model.OnTurnChanged += HandleTurnChanged;
        model.OnCardPlayed += HandleCardPlayed;
        model.OnTrickEnded += HandleTrickEnded;
        model.OnRoundOver += HandleRoundOver;
        model.OnDeckClosed += HandleDeckClosed;
        model.OnNotification += HandleNotification;
        model.OnStateChanged += HandleDisableDeckView;
        model.OnMatchOver += HandleMatchOver;

        model.ForceFullUpdate();
    }

    private void HandleDeckClosed(int playerID)
    {
        deckView.EnableDeckView(false);
        deckView.CloseDeck(true);
    }

    public void CloseDeck()
    {
        int playerID = model.GetActivePlayer();
        model.RequestCloseDeck(playerID);
    }

    public void PlayCard(int playerID, int cardIndex)
    {
        if (playerID != model.GetActivePlayer())
            return;
        model.RequestPlayCard(playerID, cardIndex);
    }

    public void ExchangeKoz()
    {
        int playerID = model.GetActivePlayer();
        model.RequestExchangeKoz(playerID);
    }

    private void RestartRound()
    {
        model.RestartRound();
    }

    private void RestartMatch()
    {
        model.RestartMatch();
    }


    private void HandleHandChanged(int playerID, List<Card> hand)
    {
        handView.UpdateHand(playerID, hand);
    }

    private void HandleKozChanged(Card koz)
    {
        kozView.UpdateKoz(koz);
    }

    private void HandleDisableDeckView()
    {
        deckView.EnableDeckView(false);
    }

    private void HandleScoreChanged(int p1, int p2)
    {
        scoreView.UpdateRoundScore(p1, p2);
    }

    private void HandleTurnChanged(int playerID)
    {
        turnView.UpdateTurn(playerID);
    }

    private void HandleTrickEnded()
    {
       StartCoroutine(playedCardView.ResetAfterTrick());
    }

    private void HandleCardPlayed(int playerID, Card card)
    {
        playedCardView.ShowCard(playerID, card);
    }

    private void HandleRoundOver(int winnerID, (int,int) gamePoints)
    {
        roundOverView.ShowWinner(winnerID);
        scoreView.UpdateGameScore(gamePoints.Item1, gamePoints.Item2);

        Invoke(nameof(RestartRound), 2f);
        Invoke(nameof(DisableRoundOverPanel), 2f);
        Invoke(nameof(EnableDeckView), 2f);
        Invoke(nameof(DisableClosedDeck), 2f);
    }

    private void HandleMatchOver(int winnerID)
    {
        Invoke(nameof(RestartMatch), 2f);
    }

    private void HandleNotification(string msg)
    {
        notificationView.ShowMessage(msg);
    }

    private void DisableRoundOverPanel()
    {
        roundOverView.DisablePanel();
    }

    private void EnableDeckView()
    {
        deckView.EnableDeckView(true);
    }

    private void DisableClosedDeck()
    {
        deckView.CloseDeck(false);
    }
}
