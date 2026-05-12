using UnityEngine;
using UnityEngine.UIElements;

[CreateAssetMenu(fileName = "Cards", menuName = "ScriptableObjects/Cards")]
public class Card_SO : ScriptableObject
{
    public string Name;
    public string Suit;
    public Sprite sprite;
}
