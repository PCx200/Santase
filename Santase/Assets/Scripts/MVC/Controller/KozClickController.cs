using UnityEngine;
using UnityEngine.UI;

public class KozClickController : MonoBehaviour
{
    void Start()
    {
        GetComponent<Button>().onClick.AddListener(OnClick);
    }

    void OnClick()
    {
        GameController.Instance.ExchangeKoz();
    }
}
