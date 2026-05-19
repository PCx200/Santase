using UnityEngine;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    private MainMenu menu;
    private MainMenuView view;

    private void Start()
    {
        menu = new MainMenu();
        view = GetComponent<MainMenuView>();

        view.Bind(this);
    }

    public void OnCreateRoom()
    {
        menu.IP = view.IP;
        menu.RoomName = view.RoomName;
        menu.Password = view.Password;

        Client.Instance.CreateRoom(menu.RoomName, menu.Password);
    }

    public void OnJoinRoom()
    {
        menu.IP = view.IP;
        menu.RoomName = view.RoomName;
        menu.Password = view.Password;

        Client.Instance.JoinRoom(menu.RoomName, menu.Password);
    }
}
