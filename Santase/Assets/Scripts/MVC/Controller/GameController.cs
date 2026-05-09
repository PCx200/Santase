using UnityEngine;
using System.Collections.Generic;

public class GameController : MonoBehaviour
{
    GameModel model;

    [Header("Views")]
    [SerializeField] HandView handView;
    [SerializeField] KozView kozView;
    [SerializeField] PlayedCardView playedCardView;
    [SerializeField] ScoreView scoreView;
    [SerializeField] TurnView turnView;
    [SerializeField] NotificationView notificationView;
    [SerializeField] RoundOverView roundOverView;

    private void Start()
    {
        model = new GameModel();

        model.OnHandChanged += HandleHandChanged;
        model.OnKozChanged += HandleKozChanged;
        model.OnScoreChanged += HandleScoreChanged;
        model.OnTurnChanged += HandleTurnChanged;
        model.OnCardPlayed += HandleCardPlayed;
        model.OnRoundOver += HandleRoundOver;
        model.OnNotification += HandleNotification;

        model.ForceFullUpdate();
    }


    public void PlayCard(int cardIndex)
    {
        int playerID = model.GetActivePlayer();
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

    private void HandleScoreChanged(int p1, int p2)
    {
        scoreView.UpdateScore(p1, p2);
    }

    private void HandleTurnChanged(int playerID)
    {
        turnView.UpdateTurn(playerID);
    }

    private void HandleCardPlayed(Card card, int playerID)
    {
        playedCardView.ShowCard(card, playerID);
    }

    private void HandleRoundOver(Player winner)
    {
        roundOverView.ShowWinner(winner.ID);

        if (winner.GetGamePoints() >= 11)
        {
            Invoke(nameof(RestartMatch), 2f);
            return;
        }

        Invoke(nameof(RestartRound), 2f);
        Invoke(nameof(DisableRoundOverPanel), 2f);

    }

    private void HandleNotification(string msg)
    {
        notificationView.ShowMessage(msg);
    }

    private void DisableRoundOverPanel()
    {
        roundOverView.DisablePanel();
    }
}
