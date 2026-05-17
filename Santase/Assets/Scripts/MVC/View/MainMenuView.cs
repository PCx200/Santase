using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuView : MonoBehaviour
{
    [SerializeField] private TMP_InputField ipField;
    [SerializeField] private TMP_InputField roomNameField;
    [SerializeField] private TMP_InputField passwordField;

    [SerializeField] private Button createButton;
    [SerializeField] private Button joinButton;

    public string IP => ipField.text;
    public string RoomName => roomNameField.text;
    public string Password => passwordField.text;

    public void Bind(MainMenuController controller)
    {
        createButton.onClick.AddListener(controller.OnCreateRoom);
        joinButton.onClick.AddListener(controller.OnJoinRoom);
    }
}
