using UnityEngine;
using UnityEngine.UI;

public class CardClickController : MonoBehaviour
{
    public int playerID;
    public int cardIndex;

    GameController controller;

    void Start()
    {
        controller = FindFirstObjectByType<GameController>();
        GetComponent<Button>().onClick.AddListener(OnClick);
    }

    void OnClick()
    {
        controller.PlayCard(playerID ,cardIndex);
    }
}
