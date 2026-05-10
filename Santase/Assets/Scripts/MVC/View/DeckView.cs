using UnityEngine;

public class DeckView : MonoBehaviour
{
    [SerializeField] GameObject deck;

    public void OnSecondPhase()
    { 
        deck.SetActive(false);
    }

}
