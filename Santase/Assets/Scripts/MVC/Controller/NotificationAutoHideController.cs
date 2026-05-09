using UnityEngine;
using TMPro;

public class NotificationAutoHideController : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI messageText;
    [SerializeField] float hideDelay = 2f;

    public void Show(string msg)
    {
        messageText.text = msg;
        CancelInvoke(nameof(Hide));
        Invoke(nameof(Hide), hideDelay);
    }

    void Hide()
    {
        messageText.text = "";
    }
}
