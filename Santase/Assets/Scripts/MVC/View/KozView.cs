using System.Collections.Generic;
using UnityEngine;

public class KozView : MonoBehaviour
{
    [SerializeField] Transform kozRoot;
    [SerializeField] List<CardPresenter> cardPresenters;

    public void UpdateKoz(Card koz)
    {
        foreach (Transform child in kozRoot)
            Destroy(child.gameObject);

        CardPresenter prefab = cardPresenters.Find(cp =>
            cp.card_SO.Name == koz.GetName() &&
            cp.card_SO.Suit == koz.GetSuit()
        );

        CardPresenter presenter = Instantiate(prefab, kozRoot);
        presenter.card = koz;

        presenter.gameObject.AddComponent<KozClickController>();
    }
}
