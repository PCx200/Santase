using UnityEngine;
using UnityEngine.UI;

public class KozClickController : MonoBehaviour
{
    GameController controller;

    void Start()
    {
        controller = FindFirstObjectByType<GameController>();
        GetComponent<Button>().onClick.AddListener(OnClick);
    }

    void OnClick()
    {
        controller.ExchangeKoz();
    }
}
