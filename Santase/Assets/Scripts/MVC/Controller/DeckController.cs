using UnityEngine;
using UnityEngine.UI;

public class DeckController : MonoBehaviour
{
    private void Start()
    {
        GetComponent<Button>().onClick.AddListener(OnClick);
    }

    public void OnClick()
    {
        GameController.Instance.CloseDeck();
    }
}
