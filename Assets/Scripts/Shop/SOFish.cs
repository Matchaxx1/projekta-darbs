using UnityEngine;

[CreateAssetMenu(fileName = "New Fish", menuName = "Fish")]
public class SOFish : ScriptableObject
{
    public int id;
    public string fishName;
    public int fishPrice;
    public Sprite fishSprite;

}
