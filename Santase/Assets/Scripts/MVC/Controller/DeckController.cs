using UnityEngine;
using UnityEngine.UI;

public class DeckController : MonoBehaviour
{
    GameController controller;

    private void Start()
    {
        controller = FindFirstObjectByType<GameController>();
        GetComponent<Button>().onClick.AddListener(OnClick);
    }

    public void OnClick()
    {
        controller.CloseDeck();
    }
}
