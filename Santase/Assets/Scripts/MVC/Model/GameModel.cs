using System;
using System.Collections.Generic;

public class GameModel
{
    // Initialization events
    public event Action OnGameStarted;

    // Gameplay events
    public event Action<int, List<Card>> OnHandChanged;
    public event Action<Card> OnKozChanged;
    public event Action<int, int> OnScoreChanged;
    public event Action<int> OnTurnChanged;
    public event Action<int, Card> OnCardPlayed;
    public event Action OnTrickEnded;
    public event Action<int, (int,int)> OnRoundOver; // <winnerID, (points1, points2)>
    public event Action<int> OnDeckClosed;
    public event Action<string> OnNotification;
    public event Action OnStateChanged;
    public event Action<int> OnMatchOver;

    public enum GameState { Preparation, Phase1, Phase2, Closed }

    private int seed;

    private Player player1 = new Player(0);
    private Player player2 = new Player(1);

    private Deck deck;

    private GameState gameState = GameState.Preparation;

    private int trick = 0;
    private int activePlayer = 0;
    private int trickLeader = -1; //Whoever got the previous hand
    private int cardsPlayed = 0;

    private int playerWhoClosed;

    private Card p1Played;
    private Card p2Played;

    private Card kozCard;


    public GameModel(int seed)
    {
        this.seed = seed;
        deck = new Deck(seed);

        gameState = GameState.Preparation;
    }

    public void StartGame()
    {
        OnGameStarted?.Invoke();
        OnScoreChanged?.Invoke(player1.GetRoundPoints(), player2.GetRoundPoints());

        InitDeck();
        DetermineKoz();
        DealInitialHands();
        PutKozAsLastCard();

        OnHandChanged?.Invoke(0, player1.GetHand());
        OnHandChanged?.Invoke(1, player2.GetHand());

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
        for (int i = 0; i < 3; i++) player1.TakeCardFromDeck(deck); OnHandChanged?.Invoke(0, player1.GetHand());
        
        for (int i = 0; i < 3; i++) player2.TakeCardFromDeck(deck); OnHandChanged?.Invoke(1, player2.GetHand());

        for (int i = 0; i < 3; i++) player1.TakeCardFromDeck(deck); OnHandChanged?.Invoke(0, player1.GetHand());

        for (int i = 0; i < 3; i++) player2.TakeCardFromDeck(deck); OnHandChanged?.Invoke(1, player2.GetHand());
    }

    private void DetermineKoz()
    {
        List<Card> temp_cards = new List<Card>();
        Queue<Card> temp_deck = new Queue<Card>(deck.GetCards());

        for (int i = 23; i >= 0; i--)
        {
            temp_cards.Add(temp_deck.Dequeue());
        }
        //THE KOZ IS ALWAYS THE 13th ELEMENT!
        kozCard = temp_cards[12];
        //Debug.Log($"KOZ IS: {temp_cards[12].GetName()} {temp_cards[12].GetSuit()}");
        //Debug.Log(deck.GetCards().Count);

        foreach (Card card in deck.GetCards())
        {
            if (card.GetSuit() == temp_cards[12].GetSuit())
            {
                card.SetKoz(true);
                //Debug.Log($"{card.GetName()} {card.GetSuit()} IS KOZ");
            }
        }
    }

    private void PutKozAsLastCard()
    {
        Stack<Card> stack = deck.GetCards();
        List<Card> topToBottom = new List<Card>(stack.Count);
        while (stack.Count > 0)
            topToBottom.Add(stack.Pop());

        if (!topToBottom.Remove(kozCard))
        {
            System.Console.WriteLine("PutKozAsLastCard: kozCard not found in deck order.");
            for (int i = topToBottom.Count - 1; i >= 0; i--)
                stack.Push(topToBottom[i]);
            return;
        }
        topToBottom.Add(kozCard);

        for (int i = topToBottom.Count - 1; i >= 0; i--)
            stack.Push(topToBottom[i]);

        //Debug.Log($"Koz at bottom of deck: {kozCard.GetName()} {kozCard.GetSuit()}");
        OnKozChanged?.Invoke(kozCard);
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

        if (cardsPlayed == 0)
            trickLeader = activePlayer;

        Card played = player.PlayCard(cardIndex);
        player.GetHand().Remove(played);

        if (playerID == 0)
            p1Played = played;
        else
            p2Played = played;

        OnCardPlayed?.Invoke(playerID, played);

        cardsPlayed++;

        if (cardsPlayed == 2)
        {
            ResolveHand();
            return;
        }

        activePlayer = 1 - activePlayer;
        OnHandChanged?.Invoke(playerID, player.GetHand());
        OnTurnChanged?.Invoke(activePlayer);
    }

    public void RequestExchangeKoz(int playerID)
    {
        if (trick == 0)
        {
            OnNotification?.Invoke("You cannot exchange the Koz during the first trick.");
            return;
        }

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

        Card card9 = player.Change9Koz(deck);
        deck.SetLast(card9);

        if (card9 == null)
        {
            OnNotification?.Invoke("You cannot exchange the koz card.");
            return;
        }

        OnHandChanged?.Invoke(playerID, player.GetHand());
        OnKozChanged?.Invoke(card9);
    }

    public void RequestCloseDeck(int playerID)
    {
        if (trick == 0)
        {
            OnNotification?.Invoke("You cannot close the Deck during the first trick.");
            return;
        }

        if (deck.GetCards().Count <= 2)
        {
            OnNotification?.Invoke("You cannot close the Deck if there are 2 cards left in the deck.");
            return;
        }

        if (playerID != trickLeader)
        {
            OnNotification?.Invoke("Not your turn.");
            return;
        }

        gameState = GameState.Closed;
        OnNotification?.Invoke($"Player: {playerID} closed the deck");
        playerWhoClosed = playerID;

        OnDeckClosed?.Invoke(playerID);

    }

    private void ResolveHand()
    {
        // PHASE 2 INVALID MOVE CHECK
        if (gameState == GameState.Phase2 || gameState == GameState.Closed)
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
                OnTrickEnded?.Invoke();
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

        else if (trickLeader == player1.ID && c1.GetSuit() != c2.GetSuit() && !c2.GetKoz())
        {
            winner = player1; loser = player2;
        }

        else if (trickLeader == player1.ID && c1.GetSuit() != c2.GetSuit() && c2.GetKoz())
        {
            winner = player2; loser = player1;
        }

        else if (c1.GetPoints() < c2.GetPoints() && c1.GetSuit() == c2.GetSuit())
        {
            winner = player2; loser = player1;
        }

        else if (trickLeader == player2.ID && c1.GetSuit() != c2.GetSuit() && !c1.GetKoz())
        {
            winner = player2; loser = player1;
        }

        else if (trickLeader == player2.ID && c1.GetSuit() != c2.GetSuit() && c1.GetKoz())
        {
            winner = player1; loser = player2;
        }


        winner.AddToRoundPoints(c1, c2);

        OnHandChanged?.Invoke(0, player1.GetHand());
        OnHandChanged?.Invoke(1, player2.GetHand());

        OnScoreChanged?.Invoke(player1.GetRoundPoints(), player2.GetRoundPoints());
        OnTrickEnded?.Invoke();


        if (gameState == GameState.Phase1 && deck.GetCards().Count >= 2)
        {
            winner.TakeCardFromDeck(deck);
            loser.TakeCardFromDeck(deck);

            OnHandChanged?.Invoke(0, player1.GetHand());
            OnHandChanged?.Invoke(1, player2.GetHand());
        }


        if (deck.IsEmpty())
        { 
            gameState = GameState.Phase2;
            OnStateChanged?.Invoke();
        }    

        Player roundWinner = DetermineRoundWinner();
        if (roundWinner != null)
        {
            OnRoundOver?.Invoke(roundWinner.ID, (player1.GetGamePoints(),player2.GetGamePoints()));
            RestartRound();
            return;
        }

        trick++;
        activePlayer = winner.ID;
        trickLeader = activePlayer;
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

        if (gameState == GameState.Closed && player1.GetHand().Count == 0 || player2.GetHand().Count == 0)
        {
            if (player1.ID == playerWhoClosed && player1.GetRoundPoints() < 66)
            {
                if (player1.GetRoundPoints() == 0) player2.AddToGamePoints(3);
                else player2.AddToGamePoints(2);

                winner = player2;

            }
            if (player2.ID == playerWhoClosed && player2.GetRoundPoints() < 66)
            {
                if (player2.GetRoundPoints() == 0) player1.AddToGamePoints(3);
                else player1.AddToGamePoints(2);

                winner = player1;

            }
        }

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
            OnMatchOver?.Invoke(winner.ID);
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

        System.Random rand = new System.Random();
        seed = rand.Next(int.MinValue, int.MaxValue);
        deck = new Deck(seed);
        InitDeck();
        DetermineKoz();
        PutKozAsLastCard();
        DealInitialHands();

        trick = 0;
        activePlayer = 0;
        cardsPlayed = 0;
        playerWhoClosed = -1;
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
    #endregion
}
