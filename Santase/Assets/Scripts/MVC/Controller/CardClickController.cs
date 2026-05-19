using UnityEngine;
using UnityEngine.UI;

public class CardClickController : MonoBehaviour
{
    public int playerID;
    public int cardIndex;


    void Start()
    {
        GetComponent<Button>().onClick.AddListener(OnClick);
    }

    void OnClick()
    {
        GameController.Instance.PlayCard(playerID ,cardIndex);
    }
}
