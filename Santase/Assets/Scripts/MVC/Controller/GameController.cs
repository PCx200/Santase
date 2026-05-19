using UnityEngine;
using System.Collections.Generic;
using OSCTools;

public class GameController : MonoBehaviour
{
    //Removed the Model -> The server will have the controll over it 
    public static GameController Instance;

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

    [Header("Controllers")]
    [SerializeField] SFXController sfxController;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {

    }

    public void PlayCard(int playerID, int cardIndex)
    {
        if (playerID != localPlayerID)
            return;

        Client.Instance.SendPlayCard(cardIndex);
    }

    public void CloseDeck()
    {
        Client.Instance.SendCloseDeck();
    }

    public void ExchangeKoz()
    {
        Client.Instance.SendExchangeKoz();
    }

    public void HandleHandChangedFromServer(int playerID, List<Card> hand)
    {
        handView.UpdateHand(playerID, hand);
    }


    public void HandleKozChangedFromServer(string name, string suit)
    {
        Card koz = new Card(name, suit, 0);
        kozView.UpdateKoz(koz);
    }

    public void HandleScoreChangedFromServer(int p1, int p2)
    {
        scoreView.UpdateRoundScore(p1, p2);
    }

    public void HandleTurnChangedFromServer(int playerID)
    {
        turnView.UpdateTurn(playerID);
    }

    public void HandleCardPlayedFromServer(int playerID, string name, string suit, int points)
    {
        Card card = new Card(name, suit, points);
        playedCardView.ShowCard(playerID, card);
        sfxController?.HandleCardPlayed(playerID, card);
    }

    public void HandleTrickEndedFromServer()
    {
        StartCoroutine(playedCardView.ResetAfterTrick());
    }

    public void HandleDeckClosedFromServer(int playerID)
    {
        deckView.EnableDeckView(false);
        deckView.CloseDeck(true);
    }

    public void HandleStateChangedFromServer()
    {
        deckView.EnableDeckView(false);
    }

    public void HandleRoundOverFromServer(int winnerID, int gp1, int gp2)
    {
        roundOverView.ShowWinner(winnerID);
        scoreView.UpdateGameScore(gp1, gp2);

        Invoke(nameof(HideRoundOverPanel), 2f);
        Invoke(nameof(EnableDeckView), 2f);
        Invoke(nameof(DisableClosedDeck), 2f);
    }

    public void HandleMatchOverFromServer(int winnerID)
    {
        // You can show a match over screen here if you want
    }

    public void HandleNotificationFromServer(string msg)
    {
        notificationView.ShowMessage(msg);
    }

    private void HideRoundOverPanel()
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
