using System;
using UnityEngine;

public class SFXController : MonoBehaviour
{
    [SerializeField] AudioSource shuffleDeckAudio;
    [SerializeField] AudioSource cardPlayAudio;

    public void Init(GameModel model)
    {
        model.OnGameStarted += HandleGameStarted;
        model.OnCardPlayed += HandleCardPlayed;
    }

    private void HandleGameStarted()
    {
        shuffleDeckAudio.Play();
    }

    private void HandleCardPlayed(int playerID, Card card)
    {
        cardPlayAudio.Play();
    }
}
