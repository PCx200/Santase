using System.Collections.Generic;
using UnityEngine;

public class KozView : MonoBehaviour
{
    [SerializeField] Transform kozRoot;
    [SerializeField] List<Card_Presenter> cardPresenters;

    public void UpdateKoz(Card koz)
    {
        foreach (Transform child in kozRoot)
            Destroy(child.gameObject);

        Card_Presenter prefab = cardPresenters.Find(cp =>
            cp.card_SO.Name == koz.GetName() &&
            cp.card_SO.Suit == koz.GetSuit()
        );

        Card_Presenter presenter = Instantiate(prefab, kozRoot);
        presenter.card = koz;

        presenter.gameObject.AddComponent<KozClickController>();
    }
}
