using UnityEngine;
using TMPro;

public class NotificationView : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI messageText;

    public void ShowMessage(string msg)
    {
        messageText.text = msg;
    }
}