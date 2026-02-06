using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ShopSlot : MonoBehaviour
{
    public SOFish SOFish;
    public TMP_Text fishName;
    public TMP_Text fishPrice;
    public Image fishSprite;
    
    private int price;
    public void Initialize(SOFish newSOFish, int price)
    {
        // Aizpilda veikala lodziņu ar visu doto informāciju.
        SOFish = newSOFish;
        fishSprite.sprite = SOFish.fishSprite;
        fishName.text = SOFish.fishName;
        fishPrice.text = SOFish.fishPrice.ToString();
    }
}
