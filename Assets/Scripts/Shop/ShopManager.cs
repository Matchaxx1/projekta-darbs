using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ShopManager : MonoBehaviour
{
    [SerializeField] private List<ShopItems> shopItems;

    [SerializeField] private ShopSlot[] shopSlots;

    // Prefer assigning this in the Inspector. If left empty we'll try to find it at Start.
    [SerializeField] private GameObject ShopCanvas;

    private void Start()
    {
        ShopCanvas.GetComponent<CanvasGroup>().alpha = 0f;

        addToShop();
    }
    public void addToShop()
    {
        for (int i = 0; i < shopItems.Count && i < shopSlots.Length; i++)
        {
            ShopItems shopItem = shopItems[i];
            shopSlots[i].Initialize(shopItem.SOFish, shopItem.price);
            shopSlots[i].gameObject.SetActive(true);

        }

        for (int i = shopItems.Count; i < shopSlots.Length; i++)
        {
            shopSlots[i].gameObject.SetActive(false);
        }
    }

}

[System.Serializable]
public class ShopItems
{
    public SOFish SOFish;
    public int price;
}
